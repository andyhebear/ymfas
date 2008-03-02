#include <iostream>
#include <vector>
#include <cmath>
#include <set>
#include "netflix.h"

using namespace std;

/*
 * ConstantAlgorithm
 * just returns the average rating in the training set
 * (roughly-precomputed)
 */
class ConstantAlgorithm : public NetflixAlgorithm
{
public:
  ConstantAlgorithm() { }

  virtual void prepare() { }

  virtual double predict_rating(int user_id, int movie_id, int year, int month, int day)
  {
    return 3.608;
  }

  virtual ~ConstantAlgorithm() { }
};

/*
 * MovieAveragesAlgorithm 
 * precomputes the average rating for each movie and 
 * uses that as the movie rating
 */
class MovieAveragesAlgorithm : public NetflixAlgorithm
{
public:
  virtual void prepare()
  {
    Netflix* nf = get_training_set();

    // skip movie 0
    movie_averages.push_back(0);
    
    double average;

    // go through each movies and compute the average
    vector<movie_rating> movie_ratings;
    for (int i = 1; i <= NUM_MOVIES; ++i)
    {
      movie_ratings.clear();
      nf->get_all_movie_ratings(i, movie_ratings);
      
      // grab the sum of the ratings
      int num_ratings = movie_ratings.size();
      int sum = 0;
      for (int j = 0; j < num_ratings; ++j)
        sum += movie_ratings[j].rating;

      // compute and push the average
      average = double(sum) / double(num_ratings);
      movie_averages.push_back(average);
    
      if ((i % 1000) == 0) cout << i << ": " << average << endl;
    }
  }   

  double predict_rating(int user_id, int movie_id, int year, int month, int day)
  {
    return movie_averages[movie_id];
  }

  virtual ~MovieAveragesAlgorithm() { }

private:
  vector<double> movie_averages;
};


int main(void)
{
  {
    ConstantAlgorithm ca;
    ca.set_validation_users_dir("../netflix_data/valid_lf/users");
    ca.set_validation_movies_dir("../netflix_data/valid_lf/movies");
  
    ca.set_training_users_dir("../netflix_data/base_lf/users");
    ca.set_training_movies_dir("../netflix_data/base_lf/movies");
    cout << "Constant averages algorithm: " << endl;
    cout << "RMSE: " << ca.compute_RMSE() << endl;
    cout << "wRMSE: " << ca.compute_weighted_RMSE() << endl;
  }

  {
    MovieAveragesAlgorithm maa;
    maa.set_validation_users_dir("../netflix_data/valid_lf/users");
    maa.set_validation_movies_dir("../netflix_data/valid_lf/movies");
    
    maa.set_training_users_dir("../netflix_data/base_lf/users");
    maa.set_training_movies_dir("../netflix_data/base_lf/movies");
    
    maa.prepare();

    cout << "Movie averages algorithm: " << endl;
    cout << "RMSE: " << maa.compute_RMSE() << endl;
    cout << "wRMSE: " << maa.compute_weighted_RMSE() << endl;
  }

  return 0;
}
  
