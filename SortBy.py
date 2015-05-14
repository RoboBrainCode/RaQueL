from Belief import Belief
def SortBy(property,p):
	'''function takes a recordList and a property to use as a key
		and return the sorted list'''
	if property == 'Belief':
		new_results = sorted(p, key=lambda k: Belief(k))
	return new_results
