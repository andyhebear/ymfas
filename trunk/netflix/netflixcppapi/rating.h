#ifndef RATING_H__
#define RATING_H__

typedef unsigned int uint;
typedef unsigned char uchar;

/** DO NOT CHANGE THESE CONSTANTS **/
#define YEAR_OFFSET  1998
#define NUM_MOVIES   17770 
#define NO_RATING    -1
/** **/

struct rating_common 
{ 
};

/*
 * class declarations
 */
/* struct user_rating
 * compact representation of a single user rating
 */
struct user_rating : public rating_common
{
  uint movie_id: 15;
  uint rating: 3;
  uint year: 4;
  uint month: 4;
  uint day: 5;

  uint get_id() const { return movie_id; }
  uint get_year() const { return year + YEAR_OFFSET; }

  // compress a date as an integer for useful comparison
  uint get_compressed_date() const 
  { 
    return (year << 9) + (month << 5) + day; 
  }

};

/* struct movie_rating
 * compact representation of a single movie rating
 */
struct movie_rating : public rating_common
{
  uint user_id: 22;
  uint rating: 3;
  uint year: 4;
  uint month: 4;
  uint day: 5;

  uint get_id() const { return user_id; }
  uint get_year() const { return year + YEAR_OFFSET; }

  // compress a date as an integer for useful comparison
  uint get_compressed_date() const 
  { 
    return (year << 9) + (month << 5) + day; 
  }
};

/* struct rating
 * contains rating and date info
 */
struct rating_info
{
  uint rating: 3;
  uint year: 4;
  uint month: 4;
  uint day: 5;

  // constructors
  rating_info(movie_rating mr) : rating(mr.rating), 
                                 year(mr.year),
                                 month(mr.month),
                                 day(mr.day)
                                 
  { }

  rating_info(user_rating r) : rating(r.rating), 
                               year(r.year), 
                               month(r.month),
                               day(r.day)
                               
  { }

  // make the rating invalid
  rating_info(bool b) : rating(0) { }
 
  rating_info() { }

  // accessors
  uint get_year() const { return year + YEAR_OFFSET; }

  // compress a date as an integer for useful comparison
  uint get_compressed_date() const 
  { 
    return (year << 9) + (month << 5) + day; 
  }
};


// comparison functions
bool movie_compare_by_user(movie_rating a, movie_rating b);
bool movie_compare_by_rating_user(movie_rating a, movie_rating b);
bool user_compare_by_rating_movie(user_rating a, user_rating b);

template <typename T>
bool compare_by_date(T a, T b)
{
  return a.get_compressed_date() < b.get_compressed_date(); 
}

#endif
