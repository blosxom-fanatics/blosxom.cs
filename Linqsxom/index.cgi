#!/bin/sh

MONO_PATH=/home/mayuki/local/mono-2.2/bin/mono

cd $(dirname $0) && LANG=ja_JP.UTF-8 $MONO_PATH Linqsxom.exe
