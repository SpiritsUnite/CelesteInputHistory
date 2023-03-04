#!/bin/bash -xe

rm -f InputHistory.zip

mkdir tmp
cp InputHistory/bin/Release/InputHistory.dll tmp/
sed 's_InputHistory/bin/Debug/__' everest.yaml > tmp/everest.yaml
cd tmp
zip ../InputHistory.zip InputHistory.dll everest.yaml

cd ..
rm -r tmp