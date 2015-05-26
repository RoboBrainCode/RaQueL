from web2rql import web2rql
from raquel import*
from itertools import imap,ifilter
import sys
def processingRQL(query):
	# print 'why'
	if web2rql(query):
		dumpFile = 'output.txt'			# filename 
		outFile = open(dumpFile, 'w')
		orig_stdout = sys.stdout
		sys.stdout = outFile
		# print query
		try:
			exec(query)
		except Exception:
			print 'no such element exists for map/filter'

		sys.stdout = orig_stdout
		outFile.close()

		outFile = open(dumpFile, 'r')
		val = outFile.read()
		outFile.close()
		val = val.replace('\n','<br>')
		return val
	else:
		return 'invalid query'

if __name__ == "__main__":
	# print processingRQL("global add\nadd=lambda n:n+2\nL=[1,2,3,4]\nprint L\niter=imap(lambda u: add(u),L)\nprint iter.next() ")
	# print processingRQL("global objects\nobjects =fetch(\"({handle:'sitting_human'})-[:`CAN_USE`]->(V)\")\naffordances=lambda n:fetch(\"({handle :'\" + n + \"'})-[:`HAS_AFFORDANCE`]->(v)\")\niter = imap( lambda u: (u) ,objects[1])\nprint iter.next()\nprint iter.next()")

	# for ex4
	# print fetch("({handle:'laptop'})-[:`HAS_MEDIA`]->(v)")
	# print processingRQL("global entities, media\nentities = lambda n: fetch(\"(v)-[:`HAS_MEDIA`]->({handle :'\" + n + \"'})\")\nmedia = lambda n:fetch(\"({handle :'\" + n + \"'})-[:`HAS_MEDIA`]->(v)\")\nind_media = lambda a: ifilter(lambda u: len(entities(u)[1])==1,(media(a))[1])\niter1= ind_media('tv')\nprint iter1.next()\njt_media = lambda a1,a2: ifilter(lambda u: len(entities(u)[1])==2 and u in media(a2)[1],(media(a1))[1])\niter2 = jt_media('tv','television_set')\nprint iter2.next()")
	print processingRQL("fetch(\"({handle:'standing_human'})-[:`CAN_USE`]->(v)\")")
	 