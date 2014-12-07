import sys
import re
from py2neo import Graph

# RaQueL 1.0 : RoboBrain Query Library
# RoboBrain Raquel Team
# Dipendra Misra
# Parts of Code Borrowed from Michela Meister, Ashesh Jain, Ayush Dubey

# Globals
graph_url = "http://ec2-54-187-76-157.us-west-2.compute.amazonaws.com:7474/db/data/"

def fetch(pattern):
	''' Takes a modified cypher pattern e.g.
            (u{name:'cup'})-[e]->(v{name:'mug'}) and returns the list of
            tuples of instantiated values of named variables. The variable
            can be omitted in which case they are not returned. However,
            before making the cypher query, we add these variables.
	'''
	graph = Graph(graph_url)
	results = graph.cypher.execute("MATCH "+pattern+" RETURN u")
	print results
	return results

fetch("(u{name:'cup'})-[e]->(v{name:'mug'})")

