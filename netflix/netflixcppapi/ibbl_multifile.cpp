#include "netflix.h"
#include "ibbl.h"
#include "ibbl_multifile.h"


/* build the user binaries in batches to 
   decrease disk access time
*/
void IBBL_MultiFile::build_user_binaries()
{
  cout << "Building User Binaries..." << endl;
  
  time_t start_time = time(NULL);

  const int user_batch_size = 20000;
  int num_full_batches = user_list.size() / user_batch_size;
    
  int_set::iterator batch_start = user_list.begin();
  int_set::iterator batch_end = batch_start;
  batch_end += user_batch_size;
  
  // build the batches, one at a time 
  for (int i = 0; i < num_full_batches; 
       ++i, batch_start = batch_end, batch_end += user_batch_size)
  {
    cout << (i + 1) << " of " << (num_full_batches + 1) << ": " << flush;
    build_user_binary_batch(batch_start, batch_end);
    cout << "done." << endl;
  }

  cout << (num_full_batches + 1) << " of " << (num_full_batches + 1) 
       << ": " << flush;
  build_user_binary_batch(batch_start, user_list.end());
  cout << "done." << endl;                          

  // print the time required to do perform the operation
  time_t diff_time = time(NULL) - start_time;

  cout << ']' << endl;
  cout << "User binaries completed building in "  
       << (diff_time / 3600) << ':';
  cout.width(2);
  cout.fill('0');
  cout << (diff_time % 3600) / 60 << ':';
  cout.width(2);
  cout.fill('0');
  cout << (diff_time % 60) << endl;
  cout.width(1);
}

void IBBL_MultiFile::build_user_binary_batch(int_set::iterator batch_start,
                                                 int_set::iterator batch_end)
{
  if (batch_start == batch_end)
    return;
  
  int_ur_mmap user_map;

  /* go through every movie file */
  MovieData d;
  movie_rating r;
  user_rating u;
  int uid;
  
  for (int id = 1; id <= NUM_MOVIES; ++id)
  {
    // load the movie
    d.load_from_binary(this, id);

    // go through each user
    for (int_set::iterator itr = batch_start; itr != batch_end; ++itr)
    {
      uid = *itr;
      //cout << " finding rating...\n" << flush;
      if (d.find_rating_by_id(uid, &r))
      {
        // convert the rating to a user rating and add to that user's list
        u.rating = r.rating;
        u.movie_id = id;
        u.year = r.year;
        u.month = r.month;
        u.day = r.day;
        user_map.insert(make_pair(uid, u));
      }
      // cout << "done finding rating...\n" << flush;
    }  
  }  
  
  /* save each user to file */
  ofstream user_file_output;
  string user_filename;
  int num_user_ratings;
  int_ur_mmap::iterator begin, end;
  for (int_set::iterator itr = batch_start; itr != batch_end; ++itr)
  {
    uid = *itr;
    user_filename = create_binary_user_filename(uid);
    user_file_output.open(user_filename.c_str(), ofstream::binary);

    if (!user_file_output.is_open())
    {
      cerr << "Couldn't write to " << user_filename << endl;
      continue;
    }
    
    pair<int_ur_mmap::iterator,int_ur_mmap::iterator> ret = user_map.equal_range(uid);
    
    // write the number of ratings
    num_user_ratings = user_map.count(uid);
    user_file_output.write((char*)&num_user_ratings, 4);
    
    // write each rating
    begin = ret.first;
    end = ret.second;
    for (int_ur_mmap::iterator u_itr = begin; u_itr != end; ++u_itr)
    {
      u = u_itr->second;
      user_file_output.write((char*)&u, sizeof(user_rating));
    }

    user_file_output.flush();
    user_file_output.close();
  }
    
}

/*
 * Build the list of all movie binary files 
 */
void IBBL_MultiFile::build_movie_binaries(bool compile_user_list)
{
  cout << "Building movie binaries:" << endl << flush;
  cout << '[';
  for (int i = 0; i < 70; ++i) cout << " ";
  cout << "]" << "\n" << "[" << flush;

  time_t start_time = time(NULL);

  const int num_movies_div_70 = NUM_MOVIES / 70;
  for (int i = 1; i <= NUM_MOVIES; ++i)
  {
    if (i % num_movies_div_70 == 0)
      cout << '*' << flush;
    convert_movie_file(i, compile_user_list);
  }

  time_t diff_time = time(NULL) - start_time;

  cout << ']' << endl;
  cout << "Movie binaries completed building in "  
       << (diff_time / 60) << ':';
  cout.width(2);
  cout.fill('0');
  cout << (diff_time % 60) << endl;
  cout.width(1);
}


/* convert a single movie file */
void IBBL_MultiFile::convert_movie_file(int movie_id, 
                                            bool compile_user_list)
{

  string movie_input_filename = create_training_set_movie_filename(movie_id);
  string movie_output_filename = create_binary_movie_filename(movie_id);

  ifstream input(movie_input_filename.c_str(), ifstream::in);
  if (!input.is_open())
  {
    cerr << "Failure reading from " << movie_input_filename << endl;
    return;
  }

  // discard the first line
  string scratch_line;
  input >> scratch_line;
  
  vector<movie_rating> ratings; 

  movie_rating r;
  int user_id = 1, rating, year, month, day;
  char scratch;

  // read each line and add the rating
  while (!input.eof())
  {
    input >> user_id >> scratch >> rating >> scratch 
          >> year >> scratch >> month >> scratch >> day;
    
    if (user_id == 1)
      continue;

    r.user_id = user_id;
    r.rating = rating;
    r.year = (year - YEAR_OFFSET);
    r.month = month;
    r.day = day;
    ratings.push_back(r);
    
    if (compile_user_list)
      user_list.insert(user_id);

    user_id = 1;
  }

  input.close();

  // open the output file
  ofstream output(movie_output_filename.c_str(), ofstream::binary);
  if (!output.is_open())
  {
    cerr << "Failure writing to " << movie_output_filename << endl;
    return; 
  }

  // sort the ratings by rating number, then user id
  sort(ratings.begin(), ratings.end(), movie_compare_by_user);

  // write the output file
  int num_ratings = ratings.size();
  output.write((char*)&num_ratings, 4);
  for (int i = 0; i < num_ratings; ++i)
    output.write((char*)(&ratings[i]), sizeof(movie_rating));
  
  output.flush();
  output.close();
}    

////////////////////////////////////////
// binary file loading methods
////////////////////////////////////////
rating_common* IBBL_MultiFile::load_user_from_binary(uint id, int& num_ratings)
{
  string filename = create_binary_user_filename(id);
  
  // assert the file is valid
  ifstream in_file(filename.c_str(), ifstream::binary);
  
  int nr;
  in_file.read((char*)&nr, 4);
  num_ratings = nr;
  
  if (num_ratings == 0)
  {
    cerr << "No file exists at " << filename << endl;
    assert("Error loading from binary file" && false);
  }

  user_rating* ratings = new user_rating[num_ratings];

  user_rating r;
  for (int i = 0; i < num_ratings; ++i)
  {
    in_file.read((char*)&r, sizeof(user_rating));
    ratings[i] = r;
  }

  return ratings;
}

rating_common* IBBL_MultiFile::load_movie_from_binary(uint id, int& num_ratings)
{  
  string filename = create_binary_movie_filename(id);

  if (id < 1 || id > NUM_MOVIES)
  {
    cerr << "Movie id " << id << " is not valid." << endl;
    assert("Error loading from binary file" && false);
  }
  
  // assert the file is valid
  ifstream in_file(filename.c_str(), ifstream::binary);
  
  int nr;
  in_file.read((char*)&nr, 4);

  num_ratings = nr;

  movie_rating* ratings = new movie_rating[num_ratings];

  movie_rating r;
  for (int i = 0; i < num_ratings; ++i)
  {
    in_file.read((char*)&r, sizeof(movie_rating));
    ratings[i] = r;
  }

  return ratings;
}



/***************************************
 *
 * filename creation
 *
 **************************************/
string IBBL_MultiFile::create_binary_movie_filename(uint movie_id)
{
  return create_filename(movies_dir, "mv", movie_id, "nfb");
}

string IBBL_MultiFile::create_binary_user_filename(uint user_id)
{
  return create_filename(users_dir, "user", user_id, "nfb");
}
