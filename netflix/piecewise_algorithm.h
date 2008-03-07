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
class PiecewiseAlgorithm : public NetflixAlgorithm
{
public:

  Netflix* nf;
  bool show_status;

  PiecewiseAlgorithm(NetflixAlgorithm *_algo1, NetflixAlgorithm *_algo2, int _cutoff);


  /*** output result files ***/
  void output_files();


   

  double predict_rating(int user_id, int movie_id, int year, int month, int day);

  string generate_filename(string mid, string suffix);

  string pred_filename();

  virtual ~PiecewiseAlgorithm();

private:
  
  NetflixAlgorithm *algo1;
  NetflixAlgorithm *algo2;
  int cutoff;
  int NUM_USERS;

  vector<int> user_list;
  map<int, int> user_id_map;
};



