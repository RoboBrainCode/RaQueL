
def Belief(p):
	'''input a record containing list of relationships
		finds the belief of the path in the record'''
	path = 0
	pathBelief = 1
	for rel in p[0]:
		belief = rel.properties['belief']
		pathBelief = min(pathBelief, belief)
	return pathBelief
	# print 'belief'
	# print pathBelief