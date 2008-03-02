CC = g++
CFLAGS = -Wall -O2
#OPTIONS = -Wall -O2
DEBUG_OPTIONS = -Wall
LIB_OBJS = ibbl.o ibbl_multifile.o ibbl_largefile.o netflix.o rating.o algorithm.o
LF_SETUP_OBJS = lf_setup.o $(LIB_OBJS)
MF_SETUP_OBJS = mf_setup.o $(LIB_OBJS)
DEMO_OBJS = netflix_demo.o $(LIB_OBJS)
ALGO_OBJS = netflix_algo_demo.o $(LIB_OBJS)

demo: netflix_demo netflix_algo_demo

.cpp.o :
	$(CC) $(CFLAGS) -c -o $@ $*.cpp

lf_setup: $(LF_SETUP_OBJS)
	$(CC) $(CFLAGS) $(LF_SETUP_OBJS) -o $@

mf_setup: $(MF_SETUP_OBJS)
	$(CC) $(CFLAGS) $(MF_SETUP_OBJS) -o $@

netflix_demo: $(DEMO_OBJS)
	$(CC) $(CFLAGS) $(DEMO_OBJS) -o $@;

netflix_algo_demo: $(ALGO_OBJS)
	$(CC) $(CFLAGS) $(ALGO_OBJS) -o $@

lib: $(LIB_OBJS)
	ar cq libnetflix.a $(LIB_OBJS)

clean: 
	rm -f *~ *.o lf_setup mf_setup netflix_demo netflix_algo_demo libnetflix.a;
