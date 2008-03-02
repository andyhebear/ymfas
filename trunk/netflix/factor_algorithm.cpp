#include <iostream>
#include <vector>
#include <cmath>
#include <set>
#include <sstream>
#include "netflix.h"
#include "newmat10/newmat.h"
#include "mat_dump.h"

using namespace std;
/*
 * class SimuFctrAlgorithm 
 * does simultaneous factorization
 */
class SimuFctrAlgorithm : public NetflixAlgorithm
{
public:

  Netflix* nf;
  bool show_status;

  SimuFctrAlgorithm(int _num_dims = 5, double _lambda = 0.3) : 
    NetflixAlgorithm(30, 30), show_status(true), NUM_DIMS(_num_dims), lambda(_lambda) 
  {
  }

  /*** load file ***/
  void prepare_from_file()
  {
    ifstream file(UV_dump_filename().c_str(), ifstream::binary);

    read_matrix_dump(file, &U);
    cout << U.Nrows() << ", " << U.Ncols() << endl;
    read_matrix_dump(file, &V);
    cout << V.Nrows() << ", " << V.Ncols() << endl;
    
    nf = get_training_set();
    nf->get_user_list(user_list);
    nf->get_seq_user_id_map(user_id_map); 
  }

  /*** compute U and V iteratively ***/
  virtual void prepare()
  {
    
    nf = get_training_set();
    nf->get_user_list(user_list);
    nf->get_seq_user_id_map(user_id_map); 

    cout << "Starting SimuFctr with " << NUM_DIMS << " factors, " 
         << "lambda = " << lambda << endl;

    srand(time(NULL));

    vector<int>::iterator begin = user_list.begin();
    vector<int>::iterator end = user_list.end();
    vector<int>::iterator itr;

    NUM_USERS = user_list.size();

    U = Matrix(NUM_USERS, NUM_DIMS);

    for (int i = 1; i <= NUM_USERS; i++) {
      for (int j = 1; j <= NUM_DIMS; j++) {
	U(i, j) = (rand() % 1000) / 1000.0;
      }
    }

    V = Matrix(NUM_MOVIES, NUM_DIMS);
 
    for (int i = 1; i <= NUM_MOVIES; i++) {
      for (int j = 1; j <= NUM_DIMS; j++) {
	V(i, j) = (rand() % 1000) / 1000.0;
      }
    }
        
    double curr_wrmse = compute_weighted_RMSE();
    double change = CHANGE_THRESHOLD + 1;
    double matrix_change = 1e6;

    for (int i = 1; i <= MAX_ITERATIONS && change > CHANGE_THRESHOLD; i++) 
    {
      cout << "Iteration #" << i << endl;

      time_t start_time = time(NULL);
     
      matrix_change = improveFactorization();
            
      double old_wrmse = curr_wrmse;
      double rmse = compute_RMSE();
      curr_wrmse = compute_weighted_RMSE();
    
      cout << "RMSE: " << rmse 
           << " ( " << rmse_to_water_level(rmse) << "% above water level)" << endl;
      cout << "Weighted RMSE: " << curr_wrmse
           << " ( " << rmse_to_water_level(curr_wrmse) << "% above water level)" << endl;
      cout << "RMSE improvement of " << (change = old_wrmse - curr_wrmse) << endl;
      
      cout << "iteration took " << time(NULL) - start_time << " seconds" << endl;
      cout << "Matrix metric change: " << matrix_change << endl << endl;
    }

  }   

  double predict_rating(int user_id, int movie_id, int year, int month, int day)
  {
    return clamp_to_range((U.Row(user_id_map[user_id]) * V.Row(movie_id).t()).AsScalar());
  }

  void dump_UV()
  {
    ofstream ou(UV_dump_filename().c_str(), ofstream::binary);
    
    dump_matrix(ou, U);
    dump_matrix(ou, V);

    ou.flush();
    ou.close();
  }

  string generate_filename(string mid, string suffix)
  { 
    ostringstream filename;
    filename << "simufctr_" << mid << "_" << lambda << "_" << NUM_DIMS << suffix;
    return filename.str();
  }

  string UV_dump_filename()
  {
    return generate_filename("uv", ".sfuv");
  }

  string pred_filename()
  {
    return generate_filename("pred", "");
  }

  virtual ~SimuFctrAlgorithm() { }

private:
  
  static const int MAX_ITERATIONS = 20;
  static const double CHANGE_THRESHOLD = 9.514e-5;
  int NUM_DIMS;
  double lambda;
  int NUM_USERS;

  vector<int> user_list;
  map<int, int> user_id_map;

  Matrix U;
  Matrix V;

  double improveFactorization() {
    double sumSquaredChange = 0;
    double change = 0;
    Matrix newRow(1, NUM_DIMS);
    
    for (int i = 1; i <= NUM_USERS; i++) {
      if (i%10000 == 1) {
	cout << "\rRecomputing U Factorization: "
             << int( 100 * (double(i) / double(NUM_USERS)) ) 
             << "%" << flush;
      }
          
      newRow = getRowU(i);
      for (int j = 1; j <= NUM_DIMS; j++) {
	change = newRow(1, j) - U(i, j);
	sumSquaredChange += change*change;
	U(i,j) = newRow(1, j);
      }
    }
         
    cout << "\rRecomputing U Factorization: 100%" << endl;
    
    for (int i = 1; i <= NUM_MOVIES; i++) {
      if (i%1000 == 1) {
	cout << "\rRecomputing V Factorization: "
             << int( 100 * (double(i) / double(NUM_MOVIES)) ) 
             << "%" << flush;
      }
      newRow = getRowV(i);
      for (int j = 1; j <= NUM_DIMS; j++) {
	change = newRow(1, j) - V(i, j);
	sumSquaredChange += change*change;
	V(i,j) = newRow(1, j);
      }
    }

    cout << "\rRecomputing V Factorization: 100%" << endl;

    return sumSquaredChange;
  }

  Matrix getRowU(int rowNum) {
   
    vector<user_rating> ratings;
    nf->get_all_user_ratings(user_list[rowNum-1], ratings);

    int size = ratings.size();

    Matrix reducedV(size, NUM_DIMS);
    Matrix reducedR(size, 1);
    
    for (int i = 0; i < size; i++) {
      reducedV.Row(i+1) = V.Row(ratings[i].movie_id);
      reducedR(i+1, 1) = ratings[i].rating;
    }
    
    IdentityMatrix lambdas(NUM_DIMS);
    lambdas *= lambda;
    
    reducedV &= lambdas;
    Matrix zero(NUM_DIMS, 1);
    zero = 0;
    reducedR &= zero;
    
    return ((reducedV.t() * reducedV).i() * (reducedV.t() * reducedR)).t();
  }


  Matrix getRowV(int rowNum) {
    vector<movie_rating> ratings;
    nf->get_all_movie_ratings(rowNum, ratings);

    int size = ratings.size();

    Matrix reducedU(size, NUM_DIMS);
    Matrix reducedR(size, 1);
    
    for (int i = 0; i < size; i++) {
      reducedU.Row(i+1) = U.Row(user_id_map[ratings[i].user_id]);
      reducedR(i+1, 1) = ratings[i].rating;
    }
   
    IdentityMatrix lambdas(NUM_DIMS);
    lambdas *= lambda;
    reducedU &= lambdas;
    Matrix zero(NUM_DIMS, 1);
    zero = 0;
    reducedR &= zero;

    return ((reducedU.t() * reducedU).i() * (reducedU.t() * reducedR)).t();
  }

};

int main(int argc, char* argv[])
{
  int num_factors = 0;
  double lambda = 0.3;
  
  bool from_file = false;

  if (argc >= 2)
    num_factors = atoi(argv[1]);
  if (argc >= 3)
    lambda = atof(argv[2]);
  if (argc >= 4)
    from_file = true;

  if (num_factors <= 0)
    num_factors = 5;
  if (0.0 >= lambda)
    lambda = 0.3;

  SimuFctrAlgorithm sfa(num_factors, lambda);
  sfa.set_validation_users_dir("../netflix_data/valid_lf/users");
  sfa.set_validation_movies_dir("../netflix_data/valid_lf/movies");
  
  sfa.set_training_users_dir("../netflix_data/both_set/users");
  sfa.set_training_movies_dir("../netflix_data/both_set/movies");

  if (from_file)
  {
    cout << "loading matrices from file" << endl;
    sfa.prepare_from_file();
    cout << "done loading" << endl;
  }
  else
  {
    sfa.prepare();
    cout << "Dumping UV" << endl;
    sfa.dump_UV();
  }

  double rmse = sfa.compute_RMSE();
  cout << "Simultaneous Factorization algorithm: " << rmse
       << " (" << sfa.rmse_to_water_level(rmse) << "% above water level)" << endl;
  cout << "Writing prediction file..." << endl;
  sfa.write_wrmse_pred_file(sfa.pred_filename());
  cout << "Writing qualifying norate file..." << endl;
  sfa.write_norate_pred_file("../netflix_data/qualifying.txt", 
                             sfa.generate_filename("norate_qual", ".txt"));
  cout << "Writing hidden norate file..." << endl;
  sfa.write_norate_pred_file("../netflix_data/hidden.norate.txt", 
                             sfa.generate_filename("norate_hidden", ".txt"));
  cout << "Writing probe norate file..." << endl;
  sfa.write_norate_pred_file("../netflix_data/probe.norate.txt", 
                             sfa.generate_filename("norate_probe", ".txt"));

  return 0;
}
