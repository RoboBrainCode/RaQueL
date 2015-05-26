from weaverParser import cyParser
from weaverWrapperFns import*
def runQuery(cypherQuery):
	# print 'we are here'
	# print cypherQuery
	start=cypherQuery.find('MATCH')
	end=cypherQuery.find('RETURN')
	end_2=cypherQuery.find('LIMIT')
	query=cypherQuery[start+5:end].strip()
	args=cypherQuery[end+6:end_2].strip()
	args=args.split(',')
	if len(args)>1:
		print 'Not Supported'
		return False
	
	# print query
	# cyParser(query)
	funcNum, dict_s, propertyList, dict_e = cyParser(query)
	# print funcNum, dict_s,propertyList,dict_e
	if funcNum == 0:
		# print funcNum
		return returnNodeOneHopForward(dict_s['handle'],{})
	elif funcNum == 1:
		# print funcNum
		return returnNodeOneHopForward(dict_s['handle'],propertyList)
	elif funcNum == 2:
		# print funcNum
		return returnNodeOneHopBackward(dict_e['handle'],{})
		# return '2'
	elif funcNum == 3:
		# print funcNum
		return returnNodeOneHopBackward(dict_e['handle'],propertyList)
		# return '3'
	elif funcNum == 4:
		# print funcNum
		print dict_s['handle'],dict_e['handle']
		# return returnPathMinMax(src=dict_s['handle'],dest=dict_e['handle'],path_len_min=1,path_len_max=1)	
	elif funcNum == 5:
		# print funcNum
		return returnPathMinMax(dict_s['handle'],dict_e['handle'],path_len_min=propertyList['start'],path_len_max=propertyList['end'])
	elif funcNum ==6:
		# print funcNum
		return returnNodesForward(src=dict_s['handle'],path_len_min=propertyList['start'],path_len_max=propertyList['end'])
	elif funcNum==7:
		# print funcNum
		return returnNodesBackward(src=dict_e['handle'],path_len_min=propertyList['start'],path_len_max=propertyList['end'])
		# return '4'
	else:
		return 'invalid_input'

	return True


if __name__ == "__main__":
	runQuery("MATCH ({handle:'wall'})-[]->(e) RETURN e")
	# runQuery("MATCH ({handle:'wall'})-[]->(e) RETURN e")

