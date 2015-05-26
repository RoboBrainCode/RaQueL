# transaction1 = " fetch ( \" ( v : `Concept` { handle : 'wall' } ) - [ e : `HAS_MATERIAL` ] -> ( b { handle : 'wall' } ) \" ) "
# transaction1 = " fetch ( \" ( v : `Concept` { handle : 'wall' } ) - [ e : `HAS_MATERIAL` ] -> ( b: `Media` { handle : 'wall' } ) \" ) "
# transaction1 = " fetch(\"(a{handle:'wall'})-[:`HAS_MATERIAL`]->(v{src:'HAS_AFFORDANCE'})\") "
# transaction2 = " objects =  fetch ( \" ( { handle : 'wall' } ) - [ e : `HAS_MATERIAL` ] -> ( b ) \" ) "


functions = ['global','fetch','SortBy','imap','ifilter','len','Belief','parents','handle','.next()','iter']

def web2rql(transaction):
	query = transaction.split('\n')
	# status = False
	for line in query:
		# print line
		status = False
		import re
		if line in ['','\n','\t','\v']:
			# print 'why'
			return True

		usr_defined_check = re.compile(r'=\s*lambda')
		m = usr_defined_check.search(line)
		if m:
			# print 'reached'
			status = regex_check(line,'usr_defined')		#for a stricter check
			if status == False:
				return False

		else:	
			for item in functions:
				if line.find(item) != -1:
					# print item
					status = regex_check(line,item)
					# print status
					if status == False:
						return False
					break
		if status == False:
			return False
	return status

# print web2rql(transaction)

def regex_check(transaction,funcName):
	import re
	# print transaction, funcName
	
	if funcName == 'global':
		'''for global the usr_defined_func'''
		return True
		

	if funcName == 'usr_defined':
		
		'''usr_defined functions can be declared 
			affordance = lambda n: fetch("{handle :'" + n + "'}) - [:`HasAffordance` ] -> (v)") '''

		usr_defined_reg = re.compile(r'[\w./,]+\s*'r'=\s*lambda'r'\s+[\w./,]+\s*:.*')
		m = usr_defined_reg.search(transaction)
		if m:
			if m.start() == 0:
				return True
			else: 
				return False
		else:
			return False	
	
	##fetch 
	elif funcName == 'fetch':
		# print 'i m in'

		## pure fetch cases
		fetch_reg = re.compile(r'(\b[\w./]*\s*=\s*)?fetch'r'\s*[(]\s*\"\s*[(]\s*(\s*([\w./]+)?\s*(:\s*`[\w./]+`)?)?\s*({\s*([\w./]+\s*:\s*\'[\w./]+\'\s*)?})?\s*[)]'r'\s*-\s*[[]\s*[\w./]*\s*(:\s*`[\w./]+`)?(\*([0-9])?\.\.([0-9])?)?\s*({\s*([\w./]+\s*:\s*\'[\w./]+\'\s*)?})?\s*[]]'r'\s*->\s*[(]\s*[\w./]*\s*(:\s*`[\w./]+`)?\s*({\s*([\w./]+\s*:\s*\'[\w./]+\'\s*)?})?\s*[)]'r'\s*\"\s*[)]\s*')

		m = fetch_reg.search(transaction)
		if m:
			if m.start() == 0:
				return True
			else: 
				return False
		else:
			return False

	elif funcName == 'Belief' or funcName == 'len':
		
		Belief_reg = re.compile(r'(\b[\w./]*\s*=\s*)?Belief|len'r'\s*[(]\s*[\w./]*\s*[)]\s*')
		m = Belief_reg.search(transaction)
		if m:
			if m.start() == 0:
				return True
			else: 
				return False
		else:
			return False

	elif funcName == 'SortBy':
		#SortBy(results,'Belief') #to call
		SortBy_reg = re.compile(r'(\b[\w./]*\s*=\s*)?SortBy'r'\s*[(]\s*[\w./]+\s*,(\'Belief\')?\s*[)]\s*')
		m = SortBy_reg.search(transaction)
		if m:
			if m.start() == 0:
				return True
			else: 
				return False
		else:
			return False

	elif funcName == 'imap':
		'''iter = imap( lambda u: affordances(u) ,objects) 	#to call
			iter.next()'''
		imap_reg = re.compile(r'(\b[\w./]*\s*=\s*)?imap'r'\s*[(]\s*lambda\s+[\w./]+:\s*')#.*,\s*[\w./]+\s*[)]\s*')
		m = imap_reg.search(transaction)
		if m:
			if m.start() == 0:
				global imap
				from itertools import imap
				return True
			else: 
				return False
		else:
			return False

	elif funcName == 'ifilter':
		'''iter = ifilter( lambda u: len(parents(u)) == 1 ,objects) 	#to call
			iter.next()'''
		imap_reg = re.compile(r'(\b[\w./]*\s*=\s*)?ifilter'r'\s*[(]\s*lambda\s+[\w./]+:\s*.*')		#not complete
		m = imap_reg.search(transaction)
		if m:
			if m.start() == 0:
				global ifilter
				from itertools import ifilter
				return True
			else: 
				return False
		else:
			return False	
	
	elif funcName == '.next()' or funcName == 'iter':
		return True

	

if __name__ == "__main__":
	# print regex_check(transaction1,'fetch')		
	# print web2rql(transaction1)
	# print web2rql('path = SortBy(results,\'Belief\')')
	# print web2rql("affordance = lambda n: fetch(\"{handle :'\" + n + \"'}) - [:`HasAffordance` ] -> (v)\")")
	# print web2rql("iter = ifilter( lambda u: affordances(u),objects)")
	# print web2rql("fetch(\"({handle:'standing_human'})-[:`CAN_USE`]->(v)\")")	
	# print web2rql("objects =fetch(\"({handle:'sitting_human'})-[:`CAN_USE`]->(V)\")\naffordances=lambda n:fetch(\"{handle :'\" + n + \"'})-[:`HAS_AFFORDANCE`]->(v)\")\niter = imap( lambda u: affordances(u) ,objects)\niter.next()")
	# print web2rql("few")
	# print web2rql("fetch(\"({handle:'sitting_human'})-[:`CAN_USE`]->(V)\")")
	# print web2rql("fetch(\"({handle:'sitting_human'})-[:`CAN_USE`]->(V)\")")
	# print web2rql("fetch(\"({handle:'standing_human'})-[:`CAN_USE`]->(v)\")\n")
	# print web2rql("fetch(\"(v)-[:`HAS_MEDIA`]->({handle:'laptop_.jpg__ozanSener/jpg/Sitting_human/laptop/heatmap_6/laptop_.jpg'})\")")
	
	# print web2rql("jt_media = lambda a1,a2: ifilter(lambda u: len(entities(u)[1])==2 and u in media(a2)[1],(media(a1))[1])")
	# print web2rql("fetch(\"(v)-[:`HAS_MEDIA`]->({handle:'tv_.jpg__ozanSener/jpg/Sitting_human/tv/heatmap_14/tv_.jpg'})\")")

	# for ex3
	# print web2rql("global affordances\nobjects =fetch(\"({handle:'sitting_human'})-[:`CAN_USE`]->(V)\")\naffordances=lambda n:fetch(\"({handle :'\" + n + \"'})-[:`HAS_MATERIAL`]->(v)\")\niter = imap( lambda u: affordances(u) ,objects[1])\nprint iter.next()\nprint iter.next()")


	#for ex2
	# print web2rql("paths =fetch(\"({handle:'standing_human'})-[e*1..3]->({handle:'phone'})\")\nSortBy(paths,'Belief')")

	#for ex4
	# print web2rql("global entities, media\nentities = lambda n: fetch(\"(v)-[:`HAS_MEDIA`]->({handle :'\" + n + \"'})\")\nmedia = lambda n:fetch(\"({handle :'\" + n + \"'})-[:`HAS_MEDIA`]->(v)\")\nind_media = lambda a: ifilter(lambda u: len(entities(u)[1])==1,(media(a))[1])\niter1= ind_media('tv')\nprint iter1.next()\njt_media = lambda a1,a2: ifilter(lambda u: len(entities(u)[1])==2 and u in media(a2)[1],(media(a1))[1])\niter2 = jt_media('tv','television_set')\nprint iter2.next()")

	#for ex1
	print web2rql("fetch(\"({handle:'standing_human'})-[:`CAN_USE`]->(v)\")")