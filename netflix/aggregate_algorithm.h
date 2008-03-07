#include <iostream>
#include <vector>
#include <cmath>
#include <set>
#include <sstream>
#include "netflix.h"
#include "newmat10/newmat.h"
#include "mat_dump.h"

using namespace std;
/*
 * class AggregateAlgorithm 
 * does simultaneous factorization
 */
class AggregateAlgorithm : public NetflixAlgorithm
{
public:

  Netflix* nf;
  bool show_status;

  AggregateAlgorithm(NetflixAlgorithm **_algo, int _num_algos, bool from_file);


  /*** output result files ***/
  void output_files();


  /*** load file ***/
  void prepare_from_file();

  /*** compute U and V iteratively ***/
  virtual void prepare();

  double predict_rating(int user_id, int movie_id, int year, int month, int day);

  void dump_UV();

  string generate_filename(string mid, string suffix);

  string UV_dump_filename();

  string pred_filename();

  virtual ~AggregateAlgorithm();

private:
  
  NetflixAlgorithm **algo;
  int NUM_ALGOS;
  double lambda;
  int NUM_USERS;

  vector<int> user_list;
  map<int, int> user_id_map;

  Matrix c;
};



