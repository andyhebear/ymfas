#include <iostream>
#include <vector>
#include <algorithm>
#include "netflix.h"

using namespace std;

void print_movie_info(Netflix* nf, int id)
{
  // we can also go from the other side, via by movie
  vector<movie_rating> movie_ratings;
  nf->get_all_movie_ratings(id, movie_ratings);

  // sort the ratings by date
  sort(movie_ratings.begin(), movie_ratings.end(), compare_by_date<movie_rating>);
  
  int num_movie_ratings = movie_ratings.size();
 
  cout << "Movie " << id << " was rated " << num_movie_ratings << " times." << endl;

  if (num_movie_ratings > 0)
  {
    movie_rating early_rating = movie_ratings[0];
    movie_rating late_rating = movie_ratings[num_movie_ratings - 1]; 
  
    cout << "The earliest rating was by user " << early_rating.user_id 
         << " on " << early_rating.get_year() << "-" << early_rating.month << '-' 
         << early_rating.day << ".\n";
    cout << "The latest rating was by user " << late_rating.user_id
         << " on " << late_rating.get_year() << "-" << late_rating.month << '-' 
         << late_rating.day << ".\n"; 
  }
}

void list_ratings_for_movie(Netflix* nf, int id, int max)
{
  vector<movie_rating> movie_ratings;
  nf->get_all_movie_ratings(id, movie_ratings);

  int num_to_print = movie_ratings.size() > max ? max : movie_ratings.size();
  for (int i = 0; i < num_to_print; ++i)
    cout << "user " << movie_ratings[i].user_id << " gave movie " << id << " a "
         << movie_ratings[i].rating << " on " 
         << movie_ratings[i].get_year() << "-" << movie_ratings[i].month << '-' 
         << movie_ratings[i].day << "." << endl;
}

int main(int argc, char** argv)
{
  // create a Netflix object
  // we can specify how many movies and users we want to keep in memory at a time
  // surprisingly, these numbers shouldn't be particularly large

  Netflix nf(30, 30, new IBBL_LargeFile());
  nf.set_users_dir("../netflix_data/base_lf/users");
  nf.set_movies_dir("../netflix_data/base_lf/movies");
  
  // grab all the ratings for a particular user
  vector<user_rating> user_ratings;
  int uid = 6;
  nf.get_all_user_ratings(uid, user_ratings);
  cout << "User " << uid << " rated " << user_ratings.size() << " movies." << endl;
  
  // grab the rating for a particular user and movie
  // ratings returned by a get_all_*_ratings are sorted by user
  user_rating ur = user_ratings[0];

  cout << "User " << uid << " gave movie " << ur.movie_id << " a " << ur.rating <<
    " on " << (ur.get_year()) << "-" << ur.month << '-' << ur.day << ".\n";

  print_movie_info(&nf, 2008);
  print_movie_info(&nf, 13924);

  return 0;
}
