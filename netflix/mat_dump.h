#ifndef MAT_DUMP_H__
#define MAT_DUMP_H__

#include "newmat10/newmat.h"
#include <iostream>
#include <fstream>

// function declarations
void dump_matrix(ofstream& o, Matrix& m);
void read_matrix_dump(ifstream& in, Matrix* m);

#endif
