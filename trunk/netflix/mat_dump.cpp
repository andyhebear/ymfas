#include "mat_dump.h"

/*
 * load a matrix from file
 */
void read_matrix_dump(ifstream& in, Matrix* m)
{
  int rows = 0;
  int cols = 0;
  in.read((char*)&rows, 4);
  in.read((char*)&cols, 4);
  cout << rows << ", " << cols << endl;
  (*m) = Matrix(rows, cols);
  
  double x = 0;

  for (int i = 1; i <= rows; ++i)
  {
    for (int j = 1; j <= cols; ++j)
    {
      in.read((char*)&x, sizeof(double));
      (*m)(i, j) = x;
    }
  }
}
/*
 * write a binary dump of the matrix to file
 */
void dump_matrix(ofstream& o, Matrix& m)
{
  const int rows = m.Nrows();
  const int cols = m.Ncols();
  o.write((char*)&rows, 4);
  o.write((char*)&cols, 4);
  
  for (int i = 1; i <= rows; ++i)
    for (int j = 1; j <= cols; ++j)
      o.write((char*)&(m(i, j)), sizeof(m(i, j)));
}
