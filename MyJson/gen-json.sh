#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
rm -rf ConsoleApp1/MyJson
mkdir -p ConsoleApp1/MyJson/Parser/Clock
java -cp aparse-2.5.jar com.parse2.aparse.Parser \
  -language cs \
  -destdir ConsoleApp1/MyJson/Parser/Clock \
  -namespace MyJson.Parser.Clock \
  json.abnf
cd $cwd/ConsoleApp1/MyJson/Parser/Clock
mv Parser.cs Parser.cs.txt
