import sys
import re
from py2neo import Graph
import csv
from cyParser import cyParser

# RaQueL 1.0 : RoboBrain Query Library
# RoboBrain Raquel Team
# Arpit Agarwal
# Parts of Code Borrowed from Dipendra Misra, Michela Meister, Ashesh Jain, Ayush Dubey

# Globals
graph_url = "http://ec2-54-187-76-157.us-west-2.compute.amazonaws.com:7474/db/data/"

def fetch(pattern):
	''' Takes a modified cypher pattern e.g.
            (a)-[:`HAS_MATERIAL`]->(b) and returns the list of
            tuples of instantiated values of named variables. The variable
            can be omitted in which case they are not returned. However,
            before making the cypher query, we add these variables.
	'''
	# print pattern
	remote_graph = Graph(graph_url)
	return_string, midQuery, propertyFound, propertyList = cyParser(pattern)
	#this if can be avoided
	if propertyFound == False:
		results = remote_graph.cypher.execute("MATCH "+ pattern +" RETURN " + return_string +" LIMIT 25")
	else:
		results = remote_graph.cypher.execute("MATCH "+ midQuery+" RETURN " + return_string +" LIMIT 25",propertyList)
	print results
	return results

# MATCH ({handle:"wall"})-[`HAS_MATERIAL`]->(b) RETURN b.handle LIMIT 25
fetch("({handle:\"wall\"})-[`HAS_MATERIAL`]->(b)")

