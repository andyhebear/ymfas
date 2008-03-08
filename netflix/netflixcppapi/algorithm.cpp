#include "netflix.h"
#include "ibbl_multifile.h"
#include <cmath>
#include <fstream>

NetflixAlgorithm::NetflixAlgorithm(int movie_cache_size, int user_cache_size)
  : training_set(movie_cache_size, user_cache_size), 
    valid_set(20, 20)
{

}

/*
 * read through all of the ratings in the 
 * valid set and compute the RMSE
 */
double NetflixAlgorithm::compute_RMSE()
{
  double se = 0.0;
  double r;
  int total_ratings = 0;

  int num_ratings_in_movie;
  vector<movie_rating> movies;
  
  // go through each movie
  for (int i = 1; i <= NUM_MOVIES; ++i)
  {      
    // grab the data
    movies.clear();
    valid_set.get_all_movie_ratings(i, movies);
    
    // add to the total number of ratings in the validation set
    total_ratings += (num_ratings_in_movie = movies.size());

    // add the squared, difference for each in the set
    for (int j = 0; j < num_ratings_in_movie; ++j)
    {
      r = predict_rating(movies[j].user_id, i, 
                         movies[j].year, movies[j].month, movies[j].day);
      se += (movies[j].rating - r) * (movies[j].rating - r);
    }
  } 

  return sqrt(se / (double)total_ratings);
}

/*
 * read through all of the ratings by movie, and compure
 * the RMSE weighted by the number of ratings
 */
double NetflixAlgorithm::compute_weighted_RMSE()
{
  double mse = 0.0;
  double r;

  int num_ratings_in_user;
  vector<user_rating> ratings;
  vector<int> user_list;

  valid_set.get_user_list(user_list);
  const int NUM_USERS = user_list.size();

  double use = 0.0;
  for (int i = 0; i < NUM_USERS; ++i)
  {
    int uid = user_list[i];

    // grab the data
    ratings.clear();
    valid_set.get_all_user_ratings(uid, ratings);

    num_ratings_in_user = ratings.size();

    // calcuate the total
    use = 0.0;
    for (int j = 0; j < num_ratings_in_user; ++j)
    {
      r = predict_rating(uid, ratings[j].movie_id, ratings[j].year, ratings[j].month, ratings[j].day);
      use += (ratings[j].rating - r) * (ratings[j].rating - r);
    }
    
    mse += use / double(num_ratings_in_user * NUM_USERS);
  }

  return sqrt(mse);
}

/*
 * create pred_file for the compute_wrmse.py file
 */
void NetflixAlgorithm::write_wrmse_pred_file(string filename)
{
  ofstream pred_file(filename.c_str());

  int num_ratings_in_movie;
  vector<movie_rating> movies;
  double r;
  
  // go through each movie
  for (int i = 1; i <= NUM_MOVIES; ++i)
  {      
    // grab the data
    movies.clear();
    valid_set.get_all_movie_ratings(i, movies);
    num_ratings_in_movie = movies.size();

    // go through each rating and print
    for (int j = 0; j < num_ratings_in_movie; ++j)
    {
      r = predict_rating(movies[j].user_id, i, 
                         movies[j].year, movies[j].month, movies[j].day);
      pred_file << i << " " << movies[j].user_id << " " << movies[j].rating 
                << " " << r << "\n";
    }
  }
  
  pred_file.flush();
  pred_file.close();
}

/*
 * create_pred_file for no rate files
 */
void NetflixAlgorithm::write_norate_pred_file(string norate, string pred)
{
  ifstream nr_file(norate.c_str());
  ofstream pred_file(pred.c_str());

  char c;
  int i = 0;
  
  nr_file >> i >> c;
  
  // as long as we are still reading movie ids
  while (i != 0)
  {
    int movie_id = i;
    
    pred_file << movie_id << ':' << endl;
    
    // read the first user
    nr_file >> i >> c;
    
    // keep reading users
    while (c == ',')
    {
      // finish reading the user
      int user_id = i;
      int year, month, day;
      nr_file >> year >> c >> month >> c >> day;
      year -= YEAR_OFFSET;

      // predict the rating, and write it to file
      pred_file << predict_rating(user_id, movie_id, year, month, day) << endl;
      
      // read the next line
      i = 0;
      nr_file >> i >> c;
    }
  }
  
  nr_file.close();
  pred_file.flush();
  pred_file.close();
}


/* 
 * user error file
 */
void NetflixAlgorithm::write_user_error_file(string error, bool use_average)
{
  ofstream error_file(error.c_str());
  double user_errors[NUM_MOVIES + 1];
  int user_rating_sizes[NUM_MOVIES + 1];
  memset(user_errors, 0, sizeof(user_errors));
  memset(user_rating_sizes, 0, sizeof(user_rating_sizes));

  int num_ratings_in_movie;
  vector<movie_rating> movies;
  double r;

  // go through each movie
  for (int i = 1; i <= NUM_MOVIES; ++i)
  {      
    // grab the data
    movies.clear();
    valid_set.get_all_movie_ratings(i, movies);
    num_ratings_in_movie = movies.size();

    // go through each rating and print
    for (int j = 0; j < num_ratings_in_movie; ++j)
    {
      r = predict_rating(movies[j].user_id, i, 
                         movies[j].year, movies[j].month, movies[j].day);
      double rating_error = pow(r - movies[j].rating, 2);
      
      int num_user_ratings = training_set.get_num_user_ratings(movies[j].user_id);

      user_errors[num_user_ratings] += rating_error;

      if (use_average)
        ++user_rating_sizes[num_user_ratings];
    }
  }

  for (int i = 1; i <= NUM_MOVIES; ++i)
  {
    error_file << 
      user_errors[i] / (use_average ? (user_rating_sizes[i] > 0 ? user_rating_sizes[i] : 1) : 1) << endl;
  }

  error_file.flush();
  error_file.close();
}
