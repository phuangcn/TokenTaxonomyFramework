#!/bin/bash
#requires protoc be installed and in your path.
#For Go, install the go plugin: https://github.com/golang/protobuf
#For ts, install the ts plugin: https://github.com/improbable-eng/ts-protoc-gen
PROTO_PATH="${PROTO_PATH:-../../../../.nuget/packages/google.protobuf.tools/3.8.0/tools}"
CSHARP_PLUGIN="${CSHARP_PLUGIN:-../../../.nuget/packages/grpc.tools/1.22.0/tools/macosx_x64/grpc_csharp_plugin}"

mkdir -p ../tools/TaxonomyObjectModel/out/csharp
mkdir -p ../tools/TaxonomyObjectModel/out/java
mkdir -p ../tools/TaxonomyObjectModel/out/go
mkdir -p ../tools/TaxonomyObjectModel/out/js
mkdir -p ../tools/TaxonomyObjectModel/out/ts

#you will need to adjust the relative path to the protoc and grpc tools.

protoc --csharp_out=../tools/TaxonomyObjectModel/out/csharp --java_out=../tools/TaxonomyObjectModel/out/java --js_out=import_style=commonjs:../tools/TaxonomyObjectModel/out/js --js_out=import_style=commonjs:../tools/TaxonomyObjectModel/out/ts --grpc-web_out=import_style=commonjs,mode=grpcwebtext:../tools/TaxonomyObjectModel/out/js --grpc-web_out=import_style=typescript,mode=grpcwebtext:../tools/TaxonomyObjectModel/out/ts --proto_path=./protos --proto_path=../../../.nuget/packages/google.protobuf.tools/3.8.0/tools ./protos/artifact.proto --plugin=protoc-gen-web
protoc --csharp_out=../tools/TaxonomyObjectModel/out/csharp --java_out=../tools/TaxonomyObjectModel/out/java --js_out=import_style=commonjs:../tools/TaxonomyObjectModel/out/js --js_out=import_style=commonjs:../tools/TaxonomyObjectModel/out/ts --grpc-web_out=import_style=commonjs,mode=grpcwebtext:../tools/TaxonomyObjectModel/out/js --grpc-web_out=import_style=typescript,mode=grpcwebtext:../tools/TaxonomyObjectModel/out/ts --proto_path=./protos --proto_path=../../../.nuget/packages/google.protobuf.tools/3.8.0/tools ./protos/core.proto --plugin=protoc-gen-web
protoc --csharp_out=../tools/TaxonomyObjectModel/out/csharp --java_out=../tools/TaxonomyObjectModel/out/java --js_out=import_style=commonjs:../tools/TaxonomyObjectModel/out/js --js_out=import_style=commonjs:../tools/TaxonomyObjectModel/out/ts --grpc-web_out=import_style=commonjs,mode=grpcwebtext:../tools/TaxonomyObjectModel/out/js --grpc-web_out=import_style=typescript,mode=grpcwebtext:../tools/TaxonomyObjectModel/out/ts --proto_path=./protos --proto_path=../../../.nuget/packages/google.protobuf.tools/3.8.0/tools ./protos/taxonomy.proto --plugin=protoc-gen-web
protoc --csharp_out=../tools/TaxonomyObjectModel/out/csharp --java_out=../tools/TaxonomyObjectModel/out/java --js_out=import_style=commonjs:../tools/TaxonomyObjectModel/out/js --js_out=import_style=commonjs:../tools/TaxonomyObjectModel/out/ts --grpc-web_out=import_style=commonjs,mode=grpcwebtext:../tools/TaxonomyObjectModel/out/js --grpc-web_out=import_style=typescript,mode=grpcwebtext:../tools/TaxonomyObjectModel/out/ts --proto_path=./protos --proto_path=../../../.nuget/packages/google.protobuf.tools/3.8.0/tools  --grpc_out ../tools/TaxonomyService/TaxonomyService ./protos/service.proto --plugin=protoc-gen-grpc=../../../.nuget/packages/grpc.tools/1.22.0/tools/macosx_x64/grpc_csharp_plugin --plugin=protoc-gen-web
protoc --proto_path=./protos --proto_path=$PROTO_PATH  --grpc_out=no_server:../tools/TaxonomyService/TaxonomyClient ./protos/service.proto --plugin=protoc-gen-grpc=$CSHARP_PLUGIN
protoc --proto_path=./protos --proto_path=$PROTO_PATH  --grpc_out=no_server:../tools/TTF-Web-Explorer/Model ./protos/service.proto --plugin=protoc-gen-grpc=$CSHARP_PLUGIN

cp ../tools/TaxonomyObjectModel/out/csharp/* ../tools/ArtifactGenerator/ArtifactGenerator/Model

cp ../tools/TaxonomyObjectModel/out/csharp/* ../tools/TaxonomyService/TaxonomyModel

cp ../tools/TaxonomyObjectModel/out/csharp/* ../tools/TTF-Web-Explorer/Model