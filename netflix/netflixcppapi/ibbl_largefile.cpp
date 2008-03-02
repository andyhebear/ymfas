#include "netflix.h"
#include "ibbl.h"
#include "ibbl_largefile.h"
#include <fstream>

IBBL_LargeFile::IBBL_LargeFile()
{
}


void IBBL_LargeFile::build_movie_binaries(bool compile_user_list)
{
  string movie_binary_filename = get_movie_binary_filename();
  string movie_offset_filename = get_movie_offset_filename();

  ofstream movie_file(movie_binary_filename.c_str(), ofstream::binary);
  ofstream movie_offset_file(movie_offset_filename.c_str(), ofstream::binary);
  
  cout << "Building movie binaries:" << endl << flush;
  cout << '[';
  for (int i = 0; i < 70; ++i) cout << " ";
  cout << "]\n[" << flush;

  time_t start_time = time(NULL);

  const int num_movies_div_70 = NUM_MOVIES / 70;
  int offset = 0;
  for (int i = 1; i <= NUM_MOVIES; ++i)
  {
    if (i % num_movies_div_70 == 0)
      cout << '*' << flush;
    append_movie(i, offset, compile_user_list,
                 movie_offset_file, movie_file);
  }

  movie_file.flush();
  movie_file.close();

  movie_offset_file.flush();
  movie_offset_file.close();

  time_t diff_time = time(NULL) - start_time;

  cout << ']' << endl;
  cout << "Movie binaries completed building in "  
       << (diff_time / 60) << ':';
  cout.width(2);
  cout.fill('0');
  cout << (diff_time % 60) << endl;
  cout.width(1);
}

/** append a movie file to the open movie binary **/
void IBBL_LargeFile::append_movie(uint id, int& offset, bool compile_user_list,
                                  ofstream& offset_file, ofstream& movie_file)
{
  string movie_input_filename = create_training_set_movie_filename(id);

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

  // sort the ratings by rating number, then user id
  sort(ratings.begin(), ratings.end(), movie_compare_by_user);

  // append the offset to the offset file 
  int num_ratings = ratings.size();
  offset_file.write((char*)&offset, 4);

  // increment the offset appropriately
  offset += 4 + ratings.size() * sizeof(movie_rating);

  // append the ratings
  movie_file.write((char*)&num_ratings, 4);
  for (int i = 0; i < num_ratings; ++i)
    movie_file.write((char*)&ratings[i], sizeof(movie_rating));
}

/*
 * assumes that the movie offsets are already loaded
 */
void IBBL_LargeFile::build_user_binaries()
{
  cout << "Building User Binaries..." << endl;
  
  time_t start_time = time(NULL);

  const int user_batch_size = 20000;
  const int num_users = user_list.size();
  int num_full_batches = num_users / user_batch_size;
    
  int_set::iterator batch_start = user_list.begin();
  int_set::iterator batch_end = batch_start;
  batch_end += user_batch_size;

  string user_filename = get_user_binary_filename();
  string user_offset_filename = get_user_offset_filename();

  ofstream user_file(user_filename.c_str(), ofstream::binary);
  ofstream user_offset_file(user_offset_filename.c_str(), ofstream::binary);
  user_offset_file.write((char*)&num_users, 4); 

  // build the batches, one at a time 
  int offset = 0;

  for (int i = 0; i < num_full_batches; 
       ++i, batch_start = batch_end, batch_end += user_batch_size)
  {
    cout << (i + 1) << " of " << (num_full_batches + 1) << ": " << flush;
    append_user_batch(batch_start, batch_end, offset, 
                      user_offset_file, user_file);
    cout << "done." << endl;
  }

  cout << (num_full_batches + 1) << " of " << (num_full_batches + 1) 
       << ": " << flush;
  int_set::iterator end = user_list.end();
  append_user_batch(batch_start, end, offset, 
                    user_offset_file, user_file);
  cout << "done." << endl;                          

  // print the time required to do perform the operation
  time_t diff_time = time(NULL) - start_time;

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

/*
 * load and write a batch of users
 */
void IBBL_LargeFile::append_user_batch(int_set::iterator& batch_start,
                                       int_set::iterator& batch_end,
                                       int& offset, 
                                       ofstream& user_offset_file,
                                       ofstream& user_file)
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
    }  
  }  
  
  /* save each user to the file */
  int num_user_ratings;
  int_ur_mmap::iterator begin, end;
  for (int_set::iterator itr = batch_start; itr != batch_end; ++itr)
  {
    uid = *itr;
    pair<int_ur_mmap::iterator,int_ur_mmap::iterator> ret = user_map.equal_range(uid);

    // write the number of ratings
    num_user_ratings = user_map.count(uid);
    user_file.write((char*)&num_user_ratings, 4);

    // write and increment the offset
    user_offset_file.write((char*)&uid, 4);
    user_offset_file.write((char*)&offset, 4);
    offset += 4 + num_user_ratings * sizeof(user_rating);

    // write each rating
    begin = ret.first;
    end = ret.second;
    for (int_ur_mmap::iterator u_itr = begin; u_itr != end; ++u_itr)
    {
      u = u_itr->second;
      user_file.write((char*)&u, sizeof(user_rating));
    }
  }
    
}

/*
 * movie binaries
 */
rating_common* IBBL_LargeFile::load_movie_from_binary(uint id, int& num_ratings)
{
  if (id < 1 || id > NUM_MOVIES)
  {
    cerr << "Movie id " << id << " is not valid." << endl;
    assert("Error loading from binary file" && false);
  }

  //ifstream movie_file(get_movie_binary_filename().c_str(), ifstream::binary);
  if (!movie_infile.is_open())
    movie_infile.open(get_movie_binary_filename().c_str(), ifstream::binary);

  rating_common* ratings = load_movie_from_binary(id, num_ratings, movie_infile);
  
  //movie_file.close();
  return ratings;
}

/*
 * user binaries
 */
rating_common* IBBL_LargeFile::load_user_from_binary(uint id, int& num_ratings) 
{ 
  //ifstream user_file(get_user_binary_filename().c_str(), ifstream::binary);
  if(!user_infile.is_open())
    user_infile.open(get_user_binary_filename().c_str(), ifstream::binary);

  rating_common* ratings = load_user_from_binary(id, num_ratings, user_infile);
  
  //user_file.close();
  return ratings;
}

rating_common* IBBL_LargeFile::load_user_from_binary(uint id, int& num_ratings,
                                                     ifstream& user_file)
{
  if (user_offsets.size() == 0)
    load_user_offsets();

  // move to the proper user offset
  user_file.seekg(user_offsets[id], ios::beg);
  
  // grab the number of ratings
  int nr;
  user_file.read((char*)&nr, 4);

  num_ratings = nr;

  user_rating* ratings = new user_rating[num_ratings];

  user_rating r;
  for (int i = 0; i < num_ratings; ++i)
  {
    user_file.read((char*)&r, sizeof(user_rating));
    ratings[i] = r;
  }

  return ratings;
}

/*
 * read the movie in from an already open movie file
 */
rating_common* IBBL_LargeFile::load_movie_from_binary(uint id, int& num_ratings, 
                                                      ifstream& movie_file)
{
  if (movie_offsets.size() == 0)
    load_movie_offsets();

  // move to the proper offset
  movie_file.seekg(movie_offsets[id], ios::beg);
  
  int nr;
  movie_file.read((char*)&nr, 4);

  num_ratings = nr;

  movie_rating* ratings = new movie_rating[num_ratings];

  movie_rating r;
  for (int i = 0; i < num_ratings; ++i)
  {
    movie_file.read((char*)&r, sizeof(movie_rating));
    ratings[i] = r;
  }

  return ratings;
}

/*
 * load the offsets from the movie binary into memory
 */
void IBBL_LargeFile::load_movie_offsets()
{
  movie_offsets.clear();
  movie_offsets.push_back(0);

  ifstream offsets(get_movie_offset_filename().c_str(), ifstream::binary);

  int offset = 0;
  for (int i = 1; i <= NUM_MOVIES; ++i)
  {
    offsets.read((char*)&offset, 4);
    movie_offsets.push_back(offset);
  }

  offsets.close();
}

/* 
 * load the offsets of the user binary into memory
 */
void IBBL_LargeFile::load_user_offsets()
{
  int num_users;
  
  user_offsets.clear();

  ifstream offsets(get_user_offset_filename().c_str(), ifstream::binary);
  offsets.read((char*)&num_users, 4);

  if (num_users == 0)
  {
    cerr << "Couldn't read user offset file: "
         << get_user_offset_filename();
    assert(false);
  }

  int id;
  int offset;
  for (int i = 0; i < num_users; ++i)
  {
    offsets.read((char*)&id, 4);
    offsets.read((char*)&offset, 4);
    user_offsets.insert(pair<int, int>(id, offset));
  }

  offsets.close();
}  

IBBL_LargeFile::~IBBL_LargeFile() 
{ 
  user_infile.close();
  movie_infile.close();
}
