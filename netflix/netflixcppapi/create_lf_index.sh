#!/bin/sh

########################################
# 
# create_lf_index.sh
# Creates the indices required for
# the Netflix C++ API
# large-file (windows) version
#
# usage:
# create_lf_index.sh
# 
# or
#
# create_lf_index.sh original_dir movies_dir users_dir
#
########################################

make lf_setup || exit;

if [ "$#" -eq "0" ]
then
    mkdir -p movies;
    mkdir -p users;
    ./lf_setup download/training_set/ movies/ users/;
    exit;
fi;

if [ "$#" -eq "3" ]
then
    mkdir -p $2;
    mkdir -p $3;
    ./lf_setup $1 $2 $3;
    exit
fi;

echo "Usage:"
echo "create_lf_index.sh"
echo "or"
echo "create_lf_index.sh ORIGINAL_DIR MOVIES_DIR USERS_DIR"

