#ifndef CACHE_HPP__
#define CACHE_HPP__

#include "ibbl.h"
#include <map>

using namespace std;

/*
 * enum cache_method
 */
enum cache_method
{
  REMOVE_OLDEST, REMOVE_FIRST
};

/* class Cache
 * FIFO cache for quick access to recently viewed data objects
 */
template<typename T>
class Cache
{
public:
  Cache(IndexBinaryBuilderLoader* _ibb, uint size) : 
    max_size(size), last_age(0), ibb(_ibb), method(REMOVE_OLDEST) { assert(size > 0); }

  // retrieve a cached object
  T& get(int id);
  T& operator[] (int id) { return get(id); }

  // clear all elements in the cache
  void clear();

  // check to see if an item has been cached
  bool is_cached(int id);
  
  // add an object to the cache if it is not already present
  void add(int id);

  ~Cache() { clear(); }

private:
  map<int, pair<T*, long> > cache_map;

  // maximum number of items the cache reaches before objects are thrown away
  uint max_size;
  long last_age;
  
  // binary builder for file loading
  IndexBinaryBuilderLoader* ibb;

  cache_method method;

  // remove the item that has been in the cache for the longest period of time
  void remove_oldest();
  void remove_first();
};

template <typename T>
void Cache<T>::clear()
{
  // delete all of the entries
  for (typename map<int, pair<T*, long> >::iterator b = cache_map.begin();
       b != cache_map.end();
       ++b)
    delete (*b).second.first;
 
  cache_map.clear();
}


/* 
 * retrieve the object from the cache
 */
template <typename T>
T& Cache<T>::get(int id)
{
  add(id);

  return *(cache_map[id].first);
}
 
/* 
 * add the data object to the cache,
 * removing an object if necessary
 */
template <typename T>
void Cache<T>::add(int id)
{
  if (is_cached(id))
    return;

  if (cache_map.size() == max_size)
    //remove_oldest();
    remove_first();

  pair<T*, long> val = pair<T*, long>(new T(ibb, id), last_age++);
  cache_map.insert(pair<int, pair<T*, long> >(id, val));
}


template <typename T>
bool Cache<T>::is_cached(int id)
{
  return (cache_map.count(id) > 0);
}

template<typename T>
bool compare_creation_time(pair<int, pair<T*, long> > a, pair<int, pair<T*, long> > b)
{
  return a.second < b.second;
}

/*
 * remove the element with the smallest creation time
 */
template <typename T>
void Cache<T>::remove_oldest()
{
  typename map<int, pair<T*, long> >::iterator begin = cache_map.begin();
  typename map<int, pair<T*, long> >::iterator end = cache_map.end();
  
  typename map<int, pair<T*, long> >::iterator min = min_element(begin, end, compare_creation_time<T>);
  delete min->second.first;
  cache_map.erase(min);
}

/*
 * remove the element with the smallest id
 */
template <typename T>
void Cache<T>::remove_first()
{
  delete cache_map.begin()->second.first;
  cache_map.erase(cache_map.begin());
}

#endif
