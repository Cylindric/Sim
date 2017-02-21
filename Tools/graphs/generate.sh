#!/bin/sh

VIEWER=i_view64.exe

while true; do
	cp graph.dot /tmp/graph.dot
	sed -i "s/\/\*selector\*\//,shape=trapezium,style=filled,fillcolor=lightskyblue3/g" /tmp/graph.dot
	sed -i "s/\/\*sequence\*\//,shape=hexagon,fillcolor=lightsalmon1,style=filled/g" /tmp/graph.dot
	sed -i "s/\/\*condition\*\//,shape=diamond/g" /tmp/graph.dot
	dot -Tpng /tmp/graph.dot -o graph.png && $VIEWER graph.png
done

