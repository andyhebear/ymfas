#include <iostream>
#include <vector>
#include <cmath>
#include <set>
#include "netflix.h"

using namespace std;

class ConstantAlgorithm : public NetflixAlgorithm
{
public:
  ConstantAlgorithm() : NetflixAlgorithm(30, 20) { }

  virtual double predict_rating(int user_id, int movie_id, int year, int month, int day)
  {
    return 4;
  }

  virtual ~ConstantAlgorithm() { }
};

class AveragesAlgorithm : public NetflixAlgorithm
{
public:
  double lambda;

  AveragesAlgorithm() : NetflixAlgorithm(30, 20), lambda(0.0) { }
  AveragesAlgorithm(double _lambda) : NetflixAlgorithm(30, 20), lambda(_lambda) { }

  virtual void prepare()
  {
    Netflix* nf = get_training_set();

    // skip movie 0
    movie_averages.push_back(0);
    double average;

    // go through each movies and compute the average
    vector<movie_rating> movie_ratings;
    nf->get_all_movie_ratings(1060, movie_ratings);

    for (int i = 1; i <= NUM_MOVIES; ++i)
    {
      average = nf->get_average_movie_rating(i);
      movie_averages.push_back(average);

      if ((i % 1000) == 0) cout << i << ": " << average << endl;
    }

    cout << "Done with movie averages" << endl;    
    vector<int> user_list;
    nf->get_user_list(user_list);
    int uid = 0;
    
    vector<int>::iterator end = user_list.end();
    vector<int>::iterator itr = user_list.begin();
    
    // go through each user and compute the avearge
    for (int i = 0; itr != end; ++itr, ++i)
    {
      uid = *itr;
      average = nf->get_average_user_rating(uid);

      user_averages.insert(pair<int, double>(uid, average));
    }

    cout << "Done with user avearges" << endl;
  }   

  double predict_rating(int user_id, int movie_id, int year, int month, int day)
  {
    //return movie_averages[movie_id];
    return movie_averages[movie_id] * lambda + user_averages[user_id] * (1.0 - lambda);
  }

  virtual ~AveragesAlgorithm() { }

private:
  vector<double> movie_averages;
  map<int, double> user_averages;
};


int main(void)
{
  AveragesAlgorithm aa;

  cout << "Created object" << endl;
  
  aa.set_validation_users_dir("../netflix_data/valid_lf/users");
  aa.set_validation_movies_dir("../netflix_data/valid_lf/movies");
  
  aa.set_training_users_dir("../netflix_data/base_lf/users");
  aa.set_training_movies_dir("../netflix_data/base_lf/movies");

  cout << "Starting preparation..." << endl;
  aa.prepare();
  cout << "Done with preparation..." << endl;

  aa.lambda = 0.50;
  cout << aa.lambda << ": RMSE of " << aa.compute_RMSE() << endl;
  cout << aa.lambda << ": wRMSE of " << aa.compute_weighted_RMSE() << endl;
  return 0;
}
  
