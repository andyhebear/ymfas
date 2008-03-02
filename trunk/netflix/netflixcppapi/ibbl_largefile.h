#ifndef IBBL_LARGEFILE_H__
#define IBBL_LARGEFILE_H__

#include <map>
#include "ibbl.h"

/* class IBBL_LargeFile
 * creates a representation of movie files using one large file
 */
class IBBL_LargeFile : public IndexBinaryBuilderLoader
{
public:
  IBBL_LargeFile();

  // binary file building
  void build_movie_binaries(bool compile_userlist);
  void build_user_binaries();

  // binary file loading
  rating_common* load_user_from_binary(uint id, int& num_ratings);
  rating_common* load_movie_from_binary(uint id, int& num_ratings);
  
  ~IBBL_LargeFile();

protected:
  // item loading helper functions
  rating_common* load_movie_from_binary(uint id, int& num_ratings, ifstream& movie_file);
  rating_common* load_user_from_binary(uint id, int& num_ratings, ifstream& user_file);

  // filename helpers
  string get_movie_offset_filename() { return movies_dir + "mv_offset.nfb"; }
  string get_user_offset_filename() { return users_dir + "user_offset.nfb"; }

  string get_movie_binary_filename() { return movies_dir + "mv.nfb"; }
  string get_user_binary_filename() { return users_dir + "user.nfb"; }

  // output helpers
  void append_movie(uint id, int& offset, bool compile_user_list,
                    ofstream& offset_file, ofstream& movie_file);
  void append_user_batch(int_set::iterator& batch_start,
                         int_set::iterator& batch_end,
                         int& offset,
                         ofstream& user_offset_file,
                         ofstream& user_file);
  
  
  // movie/user offsets
  vector<int> movie_offsets;
  map<int, int> user_offsets;

  void load_movie_offsets();
  void load_user_offsets();

  // user/movie loading files
  ifstream user_infile;
  ifstream movie_infile;
};

#endif

