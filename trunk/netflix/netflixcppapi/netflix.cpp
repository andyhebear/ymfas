#include "netflix.h"
#include <cassert>
#include <iostream>
#include <sstream>

Netflix::Netflix(IndexBinaryBuilderLoader* _ibbl) : ibb(_ibbl),
  movie_cache(ibb, 6000), user_cache(ibb, 124800)
{
  init();
}

Netflix::Netflix(int movie_cache_size, int user_cache_size,
                 IndexBinaryBuilderLoader* _ibbl)
  : ibb(_ibbl),
    movie_cache(_ibbl, movie_cache_size), user_cache(_ibbl, user_cache_size)
{
  init();
}

void Netflix::init()
{
  assert(ibb != NULL);
}

/*
 * grab rating info for a given user and movie
 */

rating_info Netflix::get_rating(uint user_id, uint movie_id)
{
  rating_info r(false);
  get_rating(user_id, movie_id, &r);
  return r;
}

bool Netflix::get_rating(uint user_id, uint movie_id, rating_info* r)
{
  // validate the user and movie ids
  assert((1 <= movie_id) && (movie_id <= NUM_MOVIES));
  
  // grab from whichever has less to search through
  if (user_cache.get(user_id).get_num_ratings() < movie_cache.get(movie_id).get_num_ratings())
  {
    // check that the rating exists
    user_rating ur;
    if (user_cache.get(user_id).find_rating_by_id(movie_id, &ur))
    {
      if (r)
        *r = rating_info(ur);
      return true;
    }
    return false;
  }
  
  // check that the rating exists
  movie_rating mr;
  if (movie_cache.get(movie_id).find_rating_by_id(user_id, &mr))
  {
      if (r)
        *r = rating_info(mr);
      return true;
  }
  return false;

}

/* grab all of the ratings from a particular user */
void Netflix::get_all_user_ratings(uint user_id, vector<user_rating>& ratings)
{
  user_cache[user_id].get_ratings(ratings);
}

void Netflix::get_all_movie_ratings(uint movie_id, vector<movie_rating>& ratings)
{
  movie_cache[movie_id].get_ratings(ratings);
}

/* grab information about ratings */
double Netflix::get_average_movie_rating(uint movie_id)
{
  return movie_cache[movie_id].get_average_rating();
}

double Netflix::get_average_user_rating(uint user_id)
{
  return user_cache[user_id].get_average_rating();
}

int Netflix::get_num_movie_ratings(uint movie_id)
{
  return movie_cache[movie_id].get_num_ratings();
}

int Netflix::get_num_user_ratings(uint user_id)
{
  return user_cache[user_id].get_num_ratings();
}

// create a map from user ids to sequential ids, starting with 1
void Netflix::get_seq_user_id_map(map<int, int>& user_id_map)
{
  user_id_map.clear();

  vector<int> user_list;
  get_user_list(user_list);
  
  int user_list_size = user_list.size();
  for (int i = 0; i < user_list_size; ++i)
    user_id_map.insert(pair<int, int>(user_list[i], i + 1)); 
}
                   
Netflix::~Netflix()
{
  delete ibb;
}
