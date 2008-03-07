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
 * class SimuFctrAlgorithm 
 * does simultaneous factorization
 */
class SimuFctrAlgorithm : public NetflixAlgorithm
{
public:

  Netflix* nf;
  bool show_status;

  SimuFctrAlgorithm(int _num_dims, double _lambda, bool from_file);

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

  virtual ~SimuFctrAlgorithm();

private:
  
  static const int MAX_ITERATIONS = 20;
  static const double CHANGE_THRESHOLD = 9.514e-5;
  int NUM_DIMS;
  double lambda;
  int NUM_USERS;

  vector<int> user_list;
  map<int, int> user_id_map;

  Matrix U;
  Matrix V;

  double improveFactorization();

  Matrix getRowU(int rowNum);

  Matrix getRowV(int rowNum);

};
