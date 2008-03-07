#include "asym_factor_algorithm.h"


using namespace std;
/*
 * class AsymFctrAlgorithm 
 * does asymmetric factorization
 */


AsymFctrAlgorithm::AsymFctrAlgorithm(int _num_dims = 5, double _lambda = 0.3, bool from_file = false) : 
  NetflixAlgorithm(30, 30), show_status(true), NUM_DIMS(_num_dims), lambda(_lambda) 
{

  set_validation_users_dir("../netflix_data/valid_lf/users");
  set_validation_movies_dir("../netflix_data/valid_lf/movies");
  
  set_training_users_dir("../netflix_data/base_lf/users");
  set_training_movies_dir("../netflix_data/base_lf/movies");

  if (from_file)
  {
    cout << "loading matrices from file" << endl;
    prepare_from_file();
    cout << "done loading" << endl;
  }
  else
  {
    prepare();
    cout << "Dumping UV" << endl;
    dump_UV();
  }
}


/*** output result files ***/
void AsymFctrAlgorithm::output_files()
{
  cout << "Writing prediction file..." << endl;
  write_wrmse_pred_file(pred_filename());
  cout << "Writing qualifying norate file..." << endl;
  write_norate_pred_file("../netflix_data/qualifying.txt", 
                             generate_filename("norate_qual", ".txt"));
  cout << "Writing hidden norate file..." << endl;
  write_norate_pred_file("../netflix_data/hidden.norate.txt", 
                             generate_filename("norate_hidden", ".txt"));
  cout << "Writing probe norate file..." << endl;
  write_norate_pred_file("../netflix_data/probe.norate.txt", 
                             generate_filename("norate_probe", ".txt"));
}

/*** load file ***/
void AsymFctrAlgorithm::prepare_from_file()
{
  ifstream file(UV_dump_filename().c_str(), ifstream::binary);

  read_matrix_dump(file, &B);
  cout << B.Nrows() << ", " << B.Ncols() << endl;
  read_matrix_dump(file, &C);
  cout << C.Nrows() << ", " << C.Ncols() << endl;
  read_matrix_dump(file, &V);
  cout << V.Nrows() << ", " << V.Ncols() << endl;
  read_matrix_dump(file, &k);
  cout << k.Nrows() << ", " << k.Ncols() << endl;
    
  nf = get_training_set();
  nf->get_user_list(user_list);
  nf->get_seq_user_id_map(user_id_map); 
}


void AsymFctrAlgorithm::prepare()
{
  nf = get_training_set();
  cout << "Starting AsymFctr with " << NUM_DIMS << " factors" << endl;
  
  srand(time(NULL));
    
  nf->get_user_list(user_list);
  nf->get_seq_user_id_map(user_id_map);

  vector<int>::iterator begin = user_list.begin();
  vector<int>::iterator end = user_list.end();
  vector<int>::iterator itr;

  NUM_USERS = user_list.size();

  B = Matrix(NUM_USERS, NUM_DIMS);

  for (int i = 1; i <= NUM_USERS; i++) {
    for (int j = 1; j <= NUM_DIMS; j++) {
      B(i, j) = (rand() % 1000) / 1000.0;
    }
  }

  C = Matrix(NUM_USERS, NUM_DIMS);

  for (int i = 1; i <= NUM_USERS; i++) {
    for (int j = 1; j <= NUM_DIMS; j++) {
      C(i, j) = (rand() % 1000) / 1000.0;
    }
  }

  
  V = Matrix(NUM_MOVIES, NUM_DIMS);
  
  for (int i = 1; i <= NUM_MOVIES; i++) {
    for (int j = 1; j <= NUM_DIMS; j++) {
      V(i, j) = (rand() % 1000) / 1000.0;
    }
  }

  k = Matrix(NUM_MOVIES, 1);
  for (int i = 1; i <= NUM_MOVIES; i++) {
    k(i, 1) = (rand() % 1000) / 1000.0;
  }

  double curr_rmse = compute_RMSE();
  double change = CHANGE_THRESHOLD + 1;
  double curr_wrmse = compute_weighted_RMSE();

  for (int i = 1; i <= MAX_ITERATIONS && change > CHANGE_THRESHOLD; i++) 
  {
    cout << "Iteration #" << i << endl;
      
    time_t start_time = time(NULL);
     
    improveFactorization();
            
    double old_wrmse = curr_wrmse;
    curr_rmse = compute_RMSE();
    curr_wrmse = compute_weighted_RMSE();
    
    cout << "RMSE: " << curr_rmse 
	 << " ( " << rmse_to_water_level(curr_rmse) << "% above water level)" << endl;
    cout << "Weighted RMSE: " << curr_wrmse
	 << " ( " << rmse_to_water_level(curr_wrmse) << "% above water level)" << endl;
    cout << "RMSE improvement of " << (change = old_wrmse - curr_wrmse) << endl;
    
    cout << "iteration took " << time(NULL) - start_time << " seconds" << endl << endl;
  }
  
}   

double AsymFctrAlgorithm::predict_rating(int user_id, int movie_id, int year, int month, int day)
{
  double val = (B.Row(user_id_map[user_id]) * V.Row(movie_id).t()).AsScalar() +
               (C.Row(user_id_map[user_id]) * V.Row(movie_id).t()).AsScalar() *
                k(movie_id,1);
  if (val > 5)
    val = 5;
  if (val < 1)
    val = 1;
  return val;

}

void AsymFctrAlgorithm::dump_UV()
{
  ofstream ou(UV_dump_filename().c_str(), ofstream::binary);
    
  dump_matrix(ou, B);
  dump_matrix(ou, C);
  dump_matrix(ou, V);
  dump_matrix(ou, k);

  ou.flush();
  ou.close();
}

string AsymFctrAlgorithm::generate_filename(string mid, string suffix)
{ 
  ostringstream filename;
  filename << "asymfctr_" << mid << "_" << lambda << "_" << NUM_DIMS << suffix;
  return filename.str();
}

string AsymFctrAlgorithm::UV_dump_filename()
{
  return generate_filename("uv", ".sfuv");
}

string AsymFctrAlgorithm::pred_filename()
{
  return generate_filename("pred", "");
}



AsymFctrAlgorithm::~AsymFctrAlgorithm() { }


  
double AsymFctrAlgorithm::improveFactorization() {
  double sumSquaredChange = 0;
  double change = 0;
  Matrix newRow(1, NUM_DIMS);
   
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
  
  for (int i = 1; i <= NUM_USERS; i++) {
    if (i%10000 == 1) {
      cout << "\rRecomputing U Factorization: "
	   << int( 100 * (double(i) / double(NUM_USERS)) ) 
	   << "%" << flush;
    }
          
    newRow = getRowU(i);
    for (int j = 1; j <= NUM_DIMS; j++) {
      change = newRow(1, j) - B(i, j);
      sumSquaredChange += change*change;
      B(i,j) = newRow(1, j);
    }
    for (int j = 1; j <= NUM_DIMS; j++) {
      change = newRow(1, j) - C(i, j);
      sumSquaredChange += change*change;
      C(i,j) = newRow(1, j+NUM_DIMS);
    }
  }
         
  cout << "\rRecomputing U Factorization: 100%" << endl;
    

  return sumSquaredChange;
}

Matrix AsymFctrAlgorithm::getRowU(int rowNum) {
   
  vector<user_rating> ratings;
  nf->get_all_user_ratings(user_list[rowNum-1], ratings);
  
  int size = ratings.size();

  Matrix reducedV(size, 2*NUM_DIMS);
  Matrix reducedR(size, 1);
    
  
  for (int i = 0; i < size; i++) {
    for (int d = 1; d <= NUM_DIMS; d++) {
      reducedV(i+1, d) = V(ratings[i].movie_id, d);
    }
    for (int d = 1; d <= NUM_DIMS; d++) {
      reducedV(i+1, d+NUM_DIMS) = V(ratings[i].movie_id, d) * k(ratings[i].movie_id, 1);
    }
    reducedR(i+1, 1) = ratings[i].rating;
  }
  
  IdentityMatrix lambdas(2*NUM_DIMS);
  lambdas *= lambda;
  
  reducedV &= lambdas;
  Matrix zero(2*NUM_DIMS, 1);
  zero = 0;
  reducedR &= zero;
    
  return ((reducedV.t() * reducedV).i() * (reducedV.t() * reducedR)).t();
}


Matrix AsymFctrAlgorithm::getRowV(int rowNum) {
  vector<movie_rating> ratings;
  nf->get_all_movie_ratings(rowNum, ratings);
  
  int size = ratings.size();
    
  Matrix reducedU(size, NUM_DIMS);
  Matrix reducedR(size, 1);

  //calculate k

  double num = 0;
  double den = 0;
  double x;
  double y;

  for (int i = 0; i < size; i++) {
    x = ratings[i].rating -
      (B.Row(user_id_map[ratings[i].user_id]) *
       V.Row(rowNum).t()).AsScalar();

    y = (C.Row(user_id_map[ratings[i].user_id]) *
	 V.Row(rowNum).t()).AsScalar();

    num += x * y;
    den += y * y;
  }

  k(rowNum,1) = num / den;


  for (int i = 0; i < size; i++) {
    reducedU.Row(i+1) = B.Row(user_id_map[ratings[i].user_id])
                      + C.Row(user_id_map[ratings[i].user_id])
                      * k(rowNum,1);

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




