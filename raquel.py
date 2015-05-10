import sys
import re
from py2neo import Graph
import csv

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
	remote_graph = Graph(graph_url)
	results = remote_graph.cypher.execute("MATCH "+pattern+" RETURN a.handle,b.handle  LIMIT 25")
	print results
	return results

fetch("(a)-[:`HAS_MATERIAL`]->(b)")

