#include "rating.h"

/* comparison functions */
bool movie_compare_by_user(movie_rating a, movie_rating b)
{
  return (a.user_id < b.user_id);
}

bool movie_compare_by_rating_user(movie_rating a, movie_rating b)
{
  return (a.rating < b.rating) ? true : 
    ((a.rating == b.rating) ? a.user_id < b.user_id : false);
}

bool user_compare_by_rating_movie(user_rating a, user_rating b)
{
  return (a.rating < b.rating) ? true : 
    ((a.rating == b.rating) ? a.movie_id < b.movie_id : false);
}

