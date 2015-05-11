def initParser(NoR):
	'''for parsing node(N) or relationship(R) from the given string 
		into string that can be returned via cypher query'''
	counter = 0
	for c in NoR:
		if c.isalpha():
			break
		counter = counter +1
	last = NoR.find(':')
	if last == 0:
		return -1
	elif last == -1:
		if counter != len(NoR):
			return NoR[counter:]
		else:
			return -1
	else:
		if counter != last-1:
			return NoR[counter:last-1]
		else:
			return NoR[counter]


def cyParser(pattern):
	'''for parsing the query into 3 parts
	starting node, edge and ending node
	then finding variables to be returned'''
	
	s1 = pattern.find('(')
	s2 = pattern.find(')')
	#starting node
	node_s = pattern[s1+1:s2]
	# print node_s
	
	r1 = pattern.find('[')
	r2 = pattern.find(']')
	#relationship 
	relationShip = pattern[r1+1:r2]
	# print relationShip

	e1 = pattern[s2+1:].find('(')
	e2 = pattern[s2+1:].find(')')
	#ending node
	node_e  = pattern[s2+1+e1+1:s2+1+e2]
	# print node_e

	#dealing with each part separately
	returnStr = ""
	insertComma = False
	init_s = initParser(node_s)
	if init_s != -1:
		returnStr  = returnStr + init_s + ".handle"
		insertComma = True
	init_r = initParser(relationShip)
	if init_r != -1:
		if insertComma:
			returnStr = returnStr + "," + init_r + ".keywords"
		else:
			returnStr = returnStr + init_r + ".keywords"
			insertComma = True

	init_e = initParser(node_e)
	if init_e != -1:
		if insertComma:
			returnStr = returnStr + "," + init_e + ".handle"
		else:
			returnStr = returnStr + init_e + ".handle"

	# print returnStr
	return returnStr
	



def main():
	# cyParser("(a)-[:`HAS_MATERIAL`]->(b)")

	# py2neo query corresponding to ({handle:\"wall\"})-[:`HAS_MATERIAL`]->(b)
	# remote_graph.cypher.execute("MATCH ({handle:{h}})-[:`HAS_MATERIAL`]->(b) RETURN b.handle LIMIT 25",{"h":"wall"}) 
	cyParser("({handle:\"wall\"})-[:`HAS_MATERIAL`]->(b)")

main()