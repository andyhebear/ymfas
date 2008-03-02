#!/bin/sh

########################################
# 
# create_mf_index.sh
# Creates the indices required for
# the Netflix C++ API
#
# usage:
# create_mf_index.sh
# 
# or
#
# create_mf_index.sh original_dir movies_dir users_dir
#
########################################

make mf_setup || exit;

if [ "$#" -eq "0" ]
then
    mkdir -p movies;
    mkdir -p users;
    ./mf_setup download/training_set/ movies/ users/;
    exit;
fi;

if [ "$#" -eq "3" ]
then
    mkdir -p $2;
    mkdir -p $3;
    ./mf_setup $1 $2 $3;
    exit
fi;

echo "Usage:"
echo "create_mf_index.sh"
echo "or"
echo "create_mf_index.sh ORIGINAL_DIR MOVIES_DIR USERS_DIR"

