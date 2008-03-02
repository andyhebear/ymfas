#ifndef IBBL_MULTIFILE_H__
#define IBBL_MULTIFILE_H__

#include <set>
#include <map>
#include "ibbl.h"
using namespace std;

/* class IBBL_MultiFile
 * responsible for building and loading movie and user binaries
 */
class IBBL_MultiFile : public IndexBinaryBuilderLoader
{
public:  
  // binary file building
  void build_movie_binaries(bool compile_userlist);
  void build_user_binaries();

  // binary file loading
  rating_common* load_user_from_binary(uint id, int& num_ratings);
  rating_common* load_movie_from_binary(uint id, int& num_ratings);

private:
  void convert_movie_file(int movie_id,
                          bool compile_user_list);

  void build_user_binary_batch(int_set::iterator batch_start,
                               int_set::iterator batch_end);
  

  string create_binary_movie_filename(uint movie_id);
  string create_binary_user_filename(uint user_id);

};

#endif
