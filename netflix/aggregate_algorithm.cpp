#include "aggregate_algorithm.h"

using namespace std;
/*
 * class AggregateAlgorithm 
 * does simultaneous factorization
 */


AggregateAlgorithm::AggregateAlgorithm(NetflixAlgorithm **_algo, int _num_algos, bool from_file) : 
  NetflixAlgorithm(30, 30), show_status(true), algo(_algo), NUM_ALGOS(_num_algos)
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
void AggregateAlgorithm::output_files()
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
void AggregateAlgorithm::prepare_from_file()
{
  ifstream file(UV_dump_filename().c_str(), ifstream::binary);

  read_matrix_dump(file, &c);
  cout << c.Nrows() << ", " << c.Ncols() << endl;
     
  nf = get_training_set();
  nf->get_user_list(user_list);
  nf->get_seq_user_id_map(user_id_map); 
}

/*** compute U and V iteratively ***/
void AggregateAlgorithm::prepare()
{
    
  nf = get_training_set();
  nf->get_user_list(user_list);
  nf->get_seq_user_id_map(user_id_map); 

  cout << "Starting aggregation with " << NUM_ALGOS << " algorithms, " 
       << endl;


  vector<int>::iterator begin = user_list.begin();
  vector<int>::iterator end = user_list.end();
  vector<int>::iterator itr;

  NUM_USERS = user_list.size();

  /*** Need to find (Mt * M)^-1 * (Mt * R)  ***/

  c = Matrix(NUM_ALGOS, 1);
  Matrix curr_row = Matrix(1, NUM_ALGOS);
  Matrix MtdotM = Matrix(NUM_ALGOS, NUM_ALGOS);
  Matrix MtdotR = Matrix(NUM_ALGOS, 1);

  MtdotM = 0;
  MtdotR = 0;

          
  for (int i = 1; i <= NUM_MOVIES; i++) {

    if (i%200 == 1) {
      cout << "\rComputing optimal aggregation: "
	   << int( 100 * (double(i) / double(NUM_MOVIES)) ) 
	   << "%" << flush;
    }

    vector<movie_rating> ratings;
    nf->get_all_movie_ratings(i, ratings);

    int size = ratings.size();

    for (int j = 0; j < size; j++) {
      for (int k = 0; k < NUM_ALGOS; k++) {
	curr_row(1, k+1) = algo[k]->predict_rating(ratings[j].user_id, 
						   i, 
						   ratings[j].year, 
						   ratings[j].month, 
						   ratings[j].day);
      }
      MtdotM += curr_row.t() * curr_row;
      MtdotR += curr_row.t() * ratings[j].rating;
    }
  }
  c = MtdotM.i() * MtdotR;

  cout << "Coefficients:" << endl;

  for (int i = 1; i <= NUM_ALGOS; i++) {
    cout << i << ": " <<  c(i, 1) << endl;
  }

}   

double AggregateAlgorithm::predict_rating(int user_id, int movie_id, int year, int month, int day)
{
  double rating = 0;
  for (int i = 0; i < NUM_ALGOS; i++) {
    rating += c(i+1, 1) * algo[i]->predict_rating(user_id, movie_id, year, month, day);
  }    
  return clamp_to_range(rating);
}

void AggregateAlgorithm::dump_UV()
{
  ofstream ou(UV_dump_filename().c_str(), ofstream::binary);
    
  dump_matrix(ou, c);

  ou.flush();
  ou.close();
}

string AggregateAlgorithm::generate_filename(string mid, string suffix)
{ 
  ostringstream filename;
  filename << "aggregation_" << mid << "_" << NUM_ALGOS << suffix;
  return filename.str();
}

string AggregateAlgorithm::UV_dump_filename()
{
  return generate_filename("c", ".sfuv");
}

string AggregateAlgorithm::pred_filename()
{
  return generate_filename("pred", "");
}

AggregateAlgorithm::~AggregateAlgorithm() { }





