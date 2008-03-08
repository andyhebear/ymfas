#include "factor_algorithm.h"
#include "asym_factor_algorithm.h"
#include "piecewise_algorithm.h"
#include "aggregate_algorithm.h"

int main(int argc, char* argv[])
{
  int num_factors = 0;
  double lambda = 0.3;
  char *alg_type = "-s";  

  bool from_file = false;

  if (argc >= 2)
    alg_type = argv[1];
  if (argc >= 3)
    num_factors = atoi(argv[2]);
  if (argc >= 4)
    lambda = atof(argv[3]);
  if (argc >= 5)
    from_file = true;

  if (num_factors <= 0)
    num_factors = 5;
  if (0.0 >= lambda)
    lambda = 0.3;


  if (!strcmp(alg_type, "-a")) {
    AsymFctrAlgorithm fa(num_factors, lambda, from_file);

    double rmse = fa.compute_weighted_RMSE();
    cout << "Asymmetric Factorization algorithm: " << rmse
	 << " (" << fa.rmse_to_water_level(rmse) << "% above water level)" << endl;

    fa.output_files();


  } else if (!strcmp(alg_type, "-ag")) {

    int num_algos = 2;
    bool from_file = false;
    NetflixAlgorithm **algo = new NetflixAlgorithm*[num_algos];

    algo[0] = new SimuFctrAlgorithm(5, 0.6, true);
    algo[1] = new AsymFctrAlgorithm(7, 0.6, true);

    AggregateAlgorithm fa(algo, num_algos, from_file);

    double rmse = fa.compute_weighted_RMSE();
    cout << "Aggregated algorithm: " << rmse
	 << " (" << fa.rmse_to_water_level(rmse) << "% above water level)" << endl;

    fa.output_files();


  } else if (!strcmp(alg_type, "-p")) {


    NetflixAlgorithm **algo = new NetflixAlgorithm*[2];

    algo[0] = new SimuFctrAlgorithm(5, 0.6, true);
    algo[1] = new SimuFctrAlgorithm(8, 0.6, true);

    PiecewiseAlgorithm fa(algo[0], algo[1], 200);

    double rmse = fa.compute_weighted_RMSE();
    cout << "Piecewise algorithm: " << rmse
	 << " (" << fa.rmse_to_water_level(rmse) << "% above water level)" << endl;

    fa.output_files();


  } else {

    SimuFctrAlgorithm fa(num_factors, lambda, from_file);
    
    double rmse = fa.compute_weighted_RMSE();
    cout << "Simultaneous Factorization algorithm: " << rmse
	 << " (" << fa.rmse_to_water_level(rmse) << "% above water level)" << endl;

    fa.output_files();

  }


  return 0;
}
