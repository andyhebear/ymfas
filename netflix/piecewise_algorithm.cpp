#include "piecewise_algorithm.h"

using namespace std;
/*
 * class AggregateAlgorithm 
 * does simultaneous factorization
 */

PiecewiseAlgorithm::PiecewiseAlgorithm(NetflixAlgorithm *_algo1, NetflixAlgorithm *_algo2, int _cutoff) : 
  NetflixAlgorithm(30, 30), show_status(true), algo1(_algo1), algo2(_algo2), cutoff(_cutoff)
{

  set_validation_users_dir("../netflix_data/valid_lf/users");
  set_validation_movies_dir("../netflix_data/valid_lf/movies");
  
  set_training_users_dir("../netflix_data/base_lf/users");
  set_training_movies_dir("../netflix_data/base_lf/movies");

  nf = get_training_set();
  nf->get_user_list(user_list);
  nf->get_seq_user_id_map(user_id_map); 

}


  /*** output result files ***/
void PiecewiseAlgorithm::output_files()
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

double PiecewiseAlgorithm::predict_rating(int user_id, int movie_id, int year, int month, int day)
{
  vector<user_rating> ratings;
  nf->get_all_user_ratings(user_id, ratings);

  if((int)ratings.size() < cutoff)
    return algo1->predict_rating(user_id, movie_id, year, month, day);
  else
    return algo2->predict_rating(user_id, movie_id, year, month, day);
   
}


string PiecewiseAlgorithm::generate_filename(string mid, string suffix)
{ 
  ostringstream filename;
  filename << "piecewise_" << mid << "_" << cutoff << suffix;
  return filename.str();
}


string PiecewiseAlgorithm::pred_filename()
{
  return generate_filename("pred", "");
}

PiecewiseAlgorithm::~PiecewiseAlgorithm() { }
