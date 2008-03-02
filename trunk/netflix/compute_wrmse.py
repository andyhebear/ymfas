#!/usr/bin/env python

import os, sys
from string import * #atoi
from math import *

argv=sys.argv
argc=len(argv)

if argc < 2:
    print "Usage: %s predict_set [user_weight]"
    print "The program computes WRMSE w.r.t. user_weight;"
    print "it computes RMSE if no user_weight file is provided."
    print
    print "Each line of the predict_set file should be of format"
    print
    print "movie_id user_id orig_rating pred_rating"
    print
    
    exit(-1)

if argc >= 3:
    user_weight = {}
    for line in open(argv[2]):
        token = split(line)
        user_weight[atoi(token[0])] = atof(token[1])

count = 0
err = 0.0

for line in map(split, open(argv[1])):
    if argc >= 3:
        uw = user_weight[atoi(line[1])]
    else:
        uw = 1.0

    count += 1
    
    err += uw * ((atof(line[2]) - atof(line[3])) ** 2)

err = sqrt(err / count)
perc = (1 - err / 0.9514) * 100

if argc >= 3:
    print "WRMSE = %f, percentage = %f"%(err, perc)
else:
    print "RMSE = %f, percentage = %f"%(err, perc)
