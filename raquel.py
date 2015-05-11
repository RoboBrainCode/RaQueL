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
	return_string = cyParser(pattern)
	# print type(return_string)
	results = remote_graph.cypher.execute("MATCH "+ pattern +" RETURN " + return_string +" LIMIT 25")
	print results
	return results

fetch("({handle:'wall'})-[:`HAS_MATERIAL`]->(b)")
# fetch("(a{handle:'wall'})-[`HAS_MATERIAL`]->(v{src:'HAS_AFFORDANCE'})")
# fetch("(v)-[`HAS_MATERIAL`]->({handle:'wall'})")

# still not supported
# fetch("({handle:'wall'})-[r*]->({handle:'cup',type:'metal'})")

