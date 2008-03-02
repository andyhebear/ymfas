#ifndef IBBL_H__
#define IBBL_H__

#include <set>
#include <map>
using namespace std;

typedef set<int> int_set;
typedef multimap<int, user_rating> int_ur_mmap;

/* class IndexBinaryBuilder
 * responsible for building and loading movie and user binaries
 */
class IndexBinaryBuilderLoader
{
public:  
  IndexBinaryBuilderLoader();

  // binary file building
  /* build the movie binary files from the movie text files */
  virtual void build_movie_binaries(bool compile_userlist) = 0;

  /* build the user binary files, given that the movie files already exist */
  virtual void build_user_binaries() = 0;

  // binary file loading
  virtual rating_common* load_user_from_binary(uint id, int& num_ratings) = 0;
  virtual rating_common* load_movie_from_binary(uint id, int& num_ratings) = 0;
  
  // user list storage
  void load_user_list();
  void save_user_list();
  void get_user_list(vector<int>& users);
  void clear_user_list();

  // directory manipulation
  string get_users_dir() const { return users_dir; }
  void set_users_dir(string _users_dir) { users_dir = _users_dir + "/"; }
  string get_movies_dir() const { return movies_dir; }
  void set_movies_dir(string _movies_dir) { movies_dir = _movies_dir + "/"; }
  string get_original_base_dir() const { return original_base_dir; }
  void set_original_base_dir(string _original_base_dir) 
  { original_base_dir = _original_base_dir + "/"; }

  virtual ~IndexBinaryBuilderLoader();

protected:
  // filename helper functions
  string create_training_set_movie_filename(uint movie_id);
  string create_filename(const string& base_dir, const string& prefix, 
                              uint id, const string& suffix);

  string get_user_list_filename() const
  {
    return users_dir + "user_list.nfb";
  }

  // where to fetch file information
  string original_base_dir;
  string movies_dir;
  string users_dir;
  

  // list of users
  int_set user_list;
};

int_set::iterator& operator+=(int_set::iterator& itr, int a);

#endif
