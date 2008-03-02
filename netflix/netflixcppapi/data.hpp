#ifndef DATA_HPP__
#define DATA_HPP__

#include "rating.h"
#include "ibbl.h"
#include <vector>

typedef rating_common* (IndexBinaryBuilderLoader::*LoadFunc)(uint, int&);

/* class Data
 * storage of a set of ratings associated with a particular item
 * templated to account for either movie or users as id-holders
 */
template<typename rating_type, LoadFunc load>
class Data
{
public:
  int num_ratings;
  rating_type* ratings;

  bool find_rating_by_id(uint id, rating_type* rating);
  int find_rating_by_id(uint id);

  Data(IndexBinaryBuilderLoader* ibb, uint id);
  Data() : num_ratings(0), ratings(NULL) { }
  
  ~Data() { clear_ratings(); }
  
  void load_from_binary(IndexBinaryBuilderLoader* ibb,uint id);

  int get_num_ratings() const { return num_ratings; }
  void get_ratings(vector<rating_type>& copy);
  void get_ratings_by_date(vector<rating_type>& date_sorted_copy);
  double get_average_rating();  

private:

  // rating clearing
  void clear_ratings();

  Data(const Data& d);
  Data& operator=(const Data& d);
};

template<typename rating_type, LoadFunc load>
Data<rating_type, load>::Data(IndexBinaryBuilderLoader* ibb, uint id) : num_ratings(0), ratings(NULL)
{
  load_from_binary(ibb, id);
}

/* 
 * returns the rating in the pointer pass to the fucntion
 * returns true if the rating was found
 */
template<typename rating_type, LoadFunc load>
bool Data<rating_type, load>::find_rating_by_id(uint id, rating_type* rating)
{
  /* binary search the user list */
  int min = 0, max = num_ratings - 1;
  int mid;
  
  while (max >= min)
  {
    mid = (max + min) / 2;
    if (ratings[mid].get_id() == id)
    {
      *rating = ratings[mid];
      return true;
    }
    if (ratings[mid].get_id() > id)
    {
      max = mid - 1;
      continue;
    }
    min = mid + 1;
  }

  return false;
}

template<typename rating_type, LoadFunc load>
int Data<rating_type, load>::find_rating_by_id(uint id)
{
  rating_type rt;
  return find_rating_by_id(id, &rt) ? rt.rating : NO_RATING;
}

/*
 * load the data from the proper binary file
 */
template<typename rating_type, LoadFunc load>
void Data<rating_type, load>::load_from_binary(IndexBinaryBuilderLoader* ibb, uint id)
{
  clear_ratings();

  ratings = (rating_type*)(ibb->*load)(id, num_ratings);
}

/*
 * grab all the ratings associated with the id
 */
template<typename rating_type, LoadFunc load>
void Data<rating_type, load>::get_ratings(vector<rating_type>& copy) 
{
  copy.clear();
  if (num_ratings > 0)
  {
    copy = vector<rating_type>(ratings, ratings + num_ratings);
  }
}

/*
 * grab all the ratings associated with an id,
 * sorted by date, starting with the oldest.
 */
template<typename rating_type, LoadFunc load>
void Data<rating_type, load>::get_ratings_by_date(vector<rating_type>& copy) 
{
  copy.clear();
  if (num_ratings > 0)
  {
    copy = vector<rating_type>(ratings, ratings + num_ratings);
    sort(copy.begin(), copy.end(), compare_by_date<rating_type>);
  }
}

/*
 * compute and return the avearge rating
 */
template<typename rating_type, LoadFunc load>
double Data<rating_type, load>::get_average_rating()
{
  if (num_ratings == 0)
    return 0.0;

  double sum = 0.0;
  for (int i = 0; i < num_ratings; ++i)
    sum += ratings[i].rating;

  return (sum / num_ratings);
}

template<typename rating_type, LoadFunc load>
void Data<rating_type, load>::clear_ratings()
{
  if (ratings != NULL)
  {
    delete [] ratings;
    ratings = NULL;
  }

  num_ratings = 0;
}


#endif
