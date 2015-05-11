def propertyParser(prop,phase):
	'''prop is the property of the query and stage is 
	whether the property is of starting node(0), relationship(1) or ending node(2)'''
	# print prop
	query = ""
	propertyList = ""
	valueStart_index = prop.find(':')
	query = query + prop[:valueStart_index+1]
	if phase ==0:
		query = query + "{s}"
		propertyList = propertyList + "\"s\":" + prop[valueStart_index+2:] 
	elif phase == 1:
		query = query + "{r}"
		propertyList = propertyList + "\"r\":" + prop[valueStart_index+1:] 
	else:
		query = query + "{e}"
		propertyList = propertyList + "\"e\":" + prop[valueStart_index+2:] 
	# print query, propertyList
	return query, propertyList

def initParser(NoR, phase):
	'''for parsing node(N) or relationship(R) from the given string 
		into string that can be returned via cypher query'''
	#initialization step
	counter = 0
	query = ""
	returnStr = ""
	propertyList = ""
	propertyFound = False
	letterFound = False

	for c in NoR:
		if c.isalpha():
			letterFound = True
			break
		if c == '{':
			propertyFound = True
			break
		if c == '`':
			break
		counter = counter +1
	
	#no identifier present
	#counter is the index from before which the property starts
	if propertyFound == True:
		propertyEnd_index = NoR.find("}")
		query, propertyList = propertyParser(NoR[counter:propertyEnd_index],phase)
		query = NoR[:counter] + query 			#assuming there is nothing left in NoR after the property 
		returnStr = ""
		return -1, query, propertyFound, propertyList

	#to deal when both letter and/or property are present
	if letterFound:
		#to check for property
		propertyStart_index = NoR.find('{')
		if propertyStart_index != -1:
			propertyFound = True

		last = NoR.find(':')	#common statement to both parts of if
		
		if propertyFound == False:
			query = NoR
			if last != -1:
				returnStr = NoR[:last]
			else:
				returnStr = NoR
			return returnStr, query, propertyFound, propertyList
		
		# when both letter and property present
		else:	
			if last > propertyStart_index:
				last = propertyStart_index
			returnStr = NoR[:last]
			propertyEnd_index = NoR.find("}")
			query, propertyList = propertyParser(NoR[propertyStart_index+1:propertyEnd_index],phase)
			query = NoR[:propertyStart_index] + "{" + query + "}"  
			return returnStr, query, propertyFound, propertyList
	else: 
		return -1, query, False, propertyList
		
	# 	if last == 0:
	# 		return -1, propertyFound
	# 	elif last == -1:
	# 		if counter != len(NoR):
	# 			return NoR[counter:], propertyFound
	# 		else:
	# 			return -1, propertyFound
	# 	else:
	# 		if counter != last-1:
	# 			return NoR[counter:last-1], propertyFound
	# 		else:
	# 			return NoR[counter], propertyFound




def cyParser(pattern):
	'''for parsing the query into 3 phase
	starting node, edge and ending node
	then finding variables to be returned'''
	
	s1 = pattern.find('(')
	s2 = pattern.find(')')
	#starting node
	node_s = pattern[s1+1:s2]
	# print node_s
	
	r1 = pattern.find('[')
	r2 = pattern.rfind(']')
	#relationship 
	relationShip = pattern[r1+1:r2]
	# print relationShip

	e1 = pattern[s2+1:].find('(')
	e2 = pattern[s2+1:].find(')')
	#ending node
	node_e  = pattern[s2+1+e1+1:s2+1+e2]
	# print node_e

	#dealing with each part separately
	#inialization of the vars to be returned
	returnStr = ""
	completePropertyList = "{"
	insertComma = False
	insertPropertyComma = False
	midQuery = ""
	
	#processing of 1 phase at a time
	init_s, query, propertyFound, propertyList = initParser(node_s,0)
	midQuery = pattern.replace(node_s, query) 
	# print midQuery
	if propertyFound:
		completePropertyList = completePropertyList + propertyList
		insertPropertyComma = True
	if init_s != -1:
		returnStr  = init_s + ".handle"
		insertComma = True
	
	init_r, query, propertyFound, propertyList = initParser(relationShip,1)
	if propertyFound:
		midQuery = midQuery.replace(relationShip, query)
		# print midQuery
		if insertPropertyComma == False:
			completePropertyList = completePropertyList + propertyList
		else:
			completePropertyList = completePropertyList + "," + propertyList
		insertPropertyComma = True
	if init_r != -1:
		if insertComma:
			returnStr = returnStr + "," + init_r + ".keywords"
		else:
			returnStr = returnStr + init_r + ".keywords"
			insertComma = True

	init_e, query, propertyFound, propertyList = initParser(node_e,2)
	if propertyFound:
		midQuery = midQuery.replace(node_e, query)
		# print midQuery
		if insertPropertyComma == False:
			completePropertyList = completePropertyList + propertyList
		else:
			completePropertyList = completePropertyList + "," + propertyList
	if init_e != -1:
		if insertComma:
			returnStr = returnStr + "," + init_e + ".handle"
		else:
			returnStr = returnStr + init_e + ".handle"

	completePropertyList = completePropertyList + "}"

	# print midQuery
	# print returnStr
	# print completePropertyList
	return returnStr, midQuery, propertyFound, completePropertyList
	



def main():
	# cyParser("(a)-[:`HAS_MATERIAL`]->(b)")

	# py2neo query corresponding to ({handle:\"wall\"})-[`HAS_MATERIAL`]->(b)
	# remote_graph.cypher.execute("MATCH ({handle:{h}})-[`HAS_MATERIAL`]->(b) RETURN b.handle LIMIT 25",{"h":"wall"}) 
	# cyParser("(a{handle:\"wall\"})-[e:`HAS_MATERIAL`{keywords:[\"Wall\",\"Wall\"]}]->(b{handle:\"wood\"})")
	cyParser("({handle:\"wall\"})-[`HAS_MATERIAL`]->(b)")
	# propertyParser("handle:\"wall\"",0)
	# propertyParser("keywords:[\"Wall\",\"Wall\"]",1)
	# initParser("{handle:\"wall\"}",0)

main()