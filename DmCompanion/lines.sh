#!/bin/bash

author=$1

git log --author="$author" --numstat --pretty=format:"%an %s %p" | grep -E '\.dart$' | grep -v '\.g\.dart$' | grep -v '\.gen\.dart$' | awk '{added+=$1; deleted+=$2} END {print "Total lines added:", added, "\nTotal lines deleted:", deleted, "\nTotal lines:", added - deleted}'
