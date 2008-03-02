#include <iostream>
#include "netflix.h"
#include "ibbl.h"
#include "ibbl_multifile.h"
#include "ibbl_largefile.h"

using namespace std;

int main(int argc, char** argv)
{
  IBBL_MultiFile ibb;

  if (argc >= 4)
  {
    ibb.set_original_base_dir(argv[1]);
    ibb.set_movies_dir(argv[2]);
    ibb.set_users_dir(argv[3]);
  }

  ibb.build_movie_binaries(true);
  
  ibb.save_user_list();
  //ibb.load_user_list();
  ibb.build_user_binaries();
}
