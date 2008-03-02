#ifndef NETFLIX_H__
#define NETFLIX_H__

#include <string>
#include <set>
#include <vector>
#include <map>
#include <fstream>
#include <iostream>
#include <cassert>
#include <string>
#include "rating.h"
#include "cache.hpp"
#include "data.hpp"
#include "ibbl.h"
#include "ibbl_multifile.h"
#include "ibbl_largefile.h"

using namespace std;

typedef Data<user_rating, &IndexBinaryBuilderLoader::load_user_from_binary> UserData;
typedef Data<movie_rating, &IndexBinaryBuilderLoader::load_movie_from_binary> MovieData;

typedef IBBL_LargeFile DefaultIBBL;

#define WATER_LEVEL    0.9514

/* class Netflix
 * handles access to training data
 * includes data caching
 */
class Netflix
{
public:
  Netflix(IndexBinaryBuilderLoader* _ibbl = new DefaultIBBL());
  Netflix(int movie_cache_size, int user_cache_size, 
          IndexBinaryBuilderLoader* _ibbl = new DefaultIBBL());

  // accessors
  rating_info get_rating(uint user_id, uint movie_id);
  bool get_rating(uint user_id, uint movie_id, rating_info* r);
  void get_all_user_ratings(uint user_id, vector<user_rating>& ratings);
  void get_all_movie_ratings(uint movie_id, vector<movie_rating>& ratings);

  // return 0.0 if there are no ratings
  double get_average_movie_rating(uint movie_id);
  double get_average_user_rating(uint user_id);

  int get_num_movie_ratings(uint movie_id);
  int get_num_user_ratings(uint user_id);

  // binary directories
  string get_users_dir() const { return ibb->get_users_dir(); }
  void set_users_dir(string _users_dir) { ibb->set_users_dir(_users_dir); }
  string get_movies_dir() const { return ibb->get_movies_dir(); }
  void set_movies_dir(string _movies_dir) { ibb->set_movies_dir(_movies_dir); }

  // grab the user list
  void get_user_list(vector<int>& users)  { ibb->get_user_list(users); }

  // create a map from user ids to sequential ids, starting with one
  void get_seq_user_id_map(map<int, int>& user_id_map);

  ~Netflix();

private:
  // prevent copying
  Netflix(const Netflix& nf);
  Netflix& operator=(const Netflix& nf);

  void init();

  // members
  typedef Cache<MovieData> MovieCache;
  typedef Cache<UserData> UserCache;
 
  IndexBinaryBuilderLoader* ibb;

  MovieCache movie_cache;
  UserCache user_cache;
  // one movie in cache is ~30 KB on average
  // one user in cache is ~0.9 Kb on average

 
};

/*** netflix algorithm ***/

class NetflixAlgorithm
{
public:
  NetflixAlgorithm(int movie_cache_size = 30, int user_cache_size = 20);
  
  /****
   * This method must be overloaded. After any required preparation, the method should
   * take all requisite information and return a rating
   ***/
  virtual double predict_rating(int user_id, int movie_id, int year, int month, int day) = 0;

  /****
   * Netflix info
   * sets the directories for the internal netflix objects
   ****/
  void set_validation_users_dir(string ud) { valid_set.set_users_dir(ud); }
  void set_validation_movies_dir(string md) { valid_set.set_movies_dir(md); }
  void set_training_users_dir(string ud) { training_set.set_users_dir(ud); }
  void set_training_movies_dir(string md) { training_set.set_movies_dir(md); }

  /*** RMSE computation ***/
  // compute the normal RMSE
  double compute_RMSE();

  // compute the RMSE, weighted by user sizes *in the validation set*
  double compute_weighted_RMSE();

  // write the prediction file for use with compute_wrmse.py
  void write_wrmse_pred_file(string filename);
  void write_norate_pred_file(string norate, string pred);

  // error files
  void write_user_total_error_file(string error)
  { write_user_error_file(error, false); }
  void write_user_average_error_file(string error)
  { write_user_error_file(error, true); }

  /*** utilities ***/
  // converts a raw rmse value to the appropriate percentage above water level
  double rmse_to_water_level(double rmse) const { return 100.0 * (1 - rmse / WATER_LEVEL); }

  double clamp_to_range(double rating) 
  { 
    return rating > 5.0 ? 5.0 : 
      (rating < 1.0 ? 1.0 :
       rating);
  }

  // returns the validation Netflix object
  Netflix* get_valid_set() { return &valid_set; }

  virtual ~NetflixAlgorithm() { }

protected:
  void write_user_error_file(string error, bool use_average);
  Netflix* get_training_set() { return &training_set; }

private:
  IndexBinaryBuilderLoader* ibb;
  
  Netflix training_set;
  Netflix valid_set;
};


#endif
