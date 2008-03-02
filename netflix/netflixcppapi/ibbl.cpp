#include <string>
#include <set>
#include <vector>
#include <iostream>
#include <fstream>
#include <sstream>
#include <algorithm>
#include <ctime>
#include <map>
#include "netflix.h"
#include "ibbl.h"

using namespace std;

IndexBinaryBuilderLoader::IndexBinaryBuilderLoader() : 
  original_base_dir("download/training_set/"),
  movies_dir("movies/"),
  users_dir("users/")
{
}

////////////////////////////////////////
// user list methods 
////////////////////////////////////////

/*
 * Load the user list from the designated user list file
 */
void IndexBinaryBuilderLoader::load_user_list()
{
  /** check to see if we already made a user file **/
  ifstream u_input(get_user_list_filename().c_str(), ifstream::binary);

  // if we have just, load it
  user_list.clear();
  int num_users = 0;
  u_input.read((char*)&num_users, 4);

  if (num_users == 0)
  {
    cerr << "Error opening " << get_user_list_filename().c_str() << endl;
    return;
  }

  int user_id;
  for (int i = 0; i < num_users; ++i)
  {
    u_input.read((char*)&user_id, 4);
    user_list.insert(user_id);
  }
  
  u_input.close();
}

/*
 * save the list of users to the designated user list file
 */
void IndexBinaryBuilderLoader::save_user_list()
{
  ofstream u_output(get_user_list_filename().c_str(), ofstream::binary);

  int num_users = user_list.size();

  cout << "Saving list of " << num_users << " users." << endl;
  
  u_output.write((char*)&num_users, 4); 
  int_set::iterator begin = user_list.begin();
  int_set::iterator end = user_list.end();

  int user_id;
  for (int_set::iterator i = begin; i != end; ++i)
  {
    user_id = *i;
    u_output.write((char*)&user_id, 4);
  }

  u_output.flush();
  u_output.close();
}

/*
 * clear the list of users to remove them from memory
 */
void IndexBinaryBuilderLoader::clear_user_list()
{
  user_list.clear();
}

/* 
 * get a copy of the user list
 */
void IndexBinaryBuilderLoader::get_user_list(vector<int>& users)
{
  if (user_list.size() == 0)
    load_user_list();
  
  users.clear();
  users.resize(user_list.size());
  copy(user_list.begin(), user_list.end(), users.begin());
}



/***************************************
 *
 * filename creation 
 *
 **************************************/
string IndexBinaryBuilderLoader::create_training_set_movie_filename(uint movie_id)
{
  return create_filename(original_base_dir, "mv", movie_id, "txt");
} 

/*
 * creates filenames of the form:
 * "[base_dir][prefix]_[id].[suffix]"
 */
string IndexBinaryBuilderLoader::create_filename(
  const string& base_dir, const string& prefix, 
  uint id, const string& suffix)
{
  ostringstream ss;
  ss << base_dir << prefix << '_';
  ss.width(7);
  ss.fill('0');
  ss << id << '.' << suffix;
  return ss.str();
}

IndexBinaryBuilderLoader::~IndexBinaryBuilderLoader()
{
}


////////////////////////////////////////
//// util
////////////////////////////////////////
int_set::iterator& operator+=(int_set::iterator& itr, int a)
{
  for (int i = 0; i < a; ++itr, ++i);
  return itr;
}
