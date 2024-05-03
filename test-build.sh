#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
version="${ts}"

cd $cwd
find . -name bin -exec rm -rf {} +
find . -name obj -exec rm -rf {} +

#cd $cwd/MyJson
#rm -rf Parser
#java -jar ./antlr-4.13.1-complete.jar JSON5.g4 -Dlanguage=CSharp -package MyJson.Parser.Json5 -o Parser/Json5
#mkdir -p Parser/Jsonc
#java -cp aparse-2.5.jar com.parse2.aparse.Parser \
#  -language cs \
#  -destdir Parser/Jsonc \
#  -namespace MyJson.Parser.Jsonc \
#  jsonc.abnf
#cd Parser/Jsonc
#ls *.cs | xargs -i sed -i "1,9d" {}
#mv Parser.cs Parser.cs.txt

cd $cwd
dotnet test -p:Configuration=Release -p:Platform="Any CPU" MyJson.sln

cd $cwd/MyJson
#sed -i -e "s/<Version>.*<\/Version>/<Version>${version}<\/Version>/g" MyJson.csproj
rm -rf *.nupkg
dotnet pack -o . -p:Configuration=Release -p:Platform="Any CPU" MyJson.csproj

exit 0

tag="MyJson-v$version"
cd $cwd
git add .
git commit -m"$tag"
git tag -a "$tag" -m"$tag"
git push origin "$tag"
git push
git remote -v
