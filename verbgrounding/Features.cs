﻿/* Tell Me Dave 2013-14, Robot-Language Learning Project
 * Code developed by - Dipendra Misra (dkm@cs.cornell.edu)
 * working in Cornell Personal Robotics Lab.
 * 
 * More details - http://tellmedave.cs.cornell.edu
 * This is Version 2.0 - it supports data version 1.1, 1.2, 1.3
 */

/*  Notes for future Developers - 
 *    <no - note > */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ProjectCompton
{
    class Features
    {
        /* Class Description : This class defines function which compute feature vectors
         * for different algorithm used in this software */

        Logger lg = null;
        private Parser obj = null;
        private Simulator sml = null;

        //Instruction Prior Feature - (Instruction, Prior)
        public List<Tuple<Instruction, double>> instructionPrior { get; private set; } //prior of every instruction

        //Verb-Correlation Feature - Controller-Function, Grounding, Position, Probability
        public List<Tuple<String, String, int, double>> verbCorrelation { get; private set; }//correlation of a description at particular point with the instruction

		//Predicate Confidence - list of predicates that have been seen along with their frequency
		private Dictionary<String,int> predicateFreq = null;
		public int zPredFreq { get; private set;} //normalization factor for predicate frequencies

		private Dictionary<String,int>[] predicateFrequencies = null;
		private Dictionary<String,int>[] argPredFrequencies = null;
		private int[] zPredicateFreq, zArgPredFreq = null;
		int maxPred = 3;

		//Giza-pp probabilities containing
		public List<Tuple<String,String,double>> gizaProbabilities { get; private set; }

		/* An environment is a conjunction of logical predicates {p}
		 * A transition probability between two environment is therefore P({p^1} | {p^2})
		 * we model this using Naive Bayes assumption that P({p^1}|{p^2}) = Prod_ij P({p^1_i} | {p^2_j}) 
		 * P({p^1_j}|{p^2_j}) is what is computed by the data-structures below. We have two forms of it, like most
		 * of other argument, one is instantiated parameter e.g., P({Grasping Robot Kettle}|{On Kettle Table})
		 * Another one is parameterized form P({Grasping x y} | {On y z}) etc. */
		public List<Tuple<String,String,double>> transitionProbability{ get; private set; } //instantiated transition values
		public List<Tuple<String,String,double>> argTransitionProbability{ get; private set; } //parameterized transition values

		//End-State Probability
		public Dictionary<String,double> endProbability{ get; private set; }
		public int zEndProb { get; private set; }

		//Relationship-Correlation
		public List<Tuple<String, SpatialRelation, int>> relCorr{ get; private set; }

        //Data-structures used by the baseline algorithm 
		public List<Template> predefinedTemplates{ get; private set; } //List of templates
        static public List<Tuple<String, String, int>> treeExp = new List<Tuple<String, String, int>>(); //Bag-of-word probability table - I apologize for being too lazy to not make it an object variable
        static public List<Tuple<Instruction, Instruction, int>> dependent = new List<Tuple<Instruction, Instruction, int>>(); //P(I1,I2), I1 comes given I2 as the last instruction

		public static List<List<String>> featureNames {get; private set; }

        public Features(Logger lg, Parser obj, Simulator sml)
        {
            this.lg = lg;
            this.obj = obj;
            this.sml = sml;
            this.instructionPrior = new List<Tuple<Instruction,double>>();
            this.verbCorrelation = new List<Tuple<string, string, int, double>>();
			this.predicateFreq = new Dictionary<string, int> ();
			this.zPredFreq = 0;

			//Environment Prior
			this.predicateFrequencies = new Dictionary<string, int>[maxPred];
			this.zPredicateFreq = Enumerable.Repeat (0, maxPred).ToArray ();
			this.zArgPredFreq = Enumerable.Repeat (0, maxPred).ToArray ();
			this.argPredFrequencies = new Dictionary<string, int>[maxPred];
			for (int i=0; i<maxPred; i++) 
			{
				this.predicateFrequencies [i] = new Dictionary<string, int> ();
				this.argPredFrequencies [i] = new Dictionary<string, int> ();
			}

			this.endProbability = new Dictionary<string, double> ();
			this.zEndProb = 0;
			this.transitionProbability = new List<Tuple<string, string, double>> ();
			this.argTransitionProbability = new List<Tuple<string, string, double>> ();
			this.predefinedTemplates = new List<Template>(); //set of predefined templates
			this.gizaProbabilities = new List<Tuple<string, string, double>> (); //giza probabilities
			this.relCorr = new List<Tuple<string, SpatialRelation, int>> (); ///relationship correlation

			//features vectors with name for different algorithms
			Features.featureNames = new List<List<string>> ();
			Features.featureNames.Add (new List<String>(){"w_obj", "w_ip", "w_dscp", "w_vcor", "w_var", "w_ling", "w_card", "w_bog", "w_trim", "w_ldscp", "w_lvcor", "w_lip"}); //Method 0
			Features.featureNames.Add (new List<String>()); //Method 1
			Features.featureNames.Add (new List<String>()); //Method 2
			Features.featureNames.Add (new List<String>()); //Method 3
			Features.featureNames.Add (new List<String>()); //Method 4
			Features.featureNames.Add( new List<String> () { "w_prior", "w_argprior", "w_dscp", "w_ll", "w_le", "w_ee", "w_sl", "w_se", "w_lerecall", "w_end", "w_sensim", "w_rel", "w_bias", "w_trans", "w_argtrans"}); //Method 5
			Features.featureNames.Add( new List<String> () { "w_prior", "w_argprior", "w_dscp", "w_ll", "w_le", "w_ee", "w_sl", "w_se", "w_lerecall", "w_end", "w_sensim", "w_rel", "w_bias", "w_trans", "w_argtrans"}); //Method 6
			Features.featureNames.Add( new List<String> () { "w_prior", "w_argprior", "w_dscp", "w_ll", "w_le", "w_ee", "w_sl", "w_se", "w_lerecall", "w_end", "w_sensim", "w_rel", "w_bias", "w_trans", "w_argtrans"}); //Method 7
			Features.featureNames.Add( new List<String> () { "w_prior", "w_argprior", "w_dscp", "w_ll", "w_le", "w_ee", "w_sl", "w_se", "w_lerecall", "w_end", "w_sensim", "w_rel", "w_bias", "w_trans", "w_argtrans"}); //Method 8
			Features.featureNames.Add( new List<String> () { "w_prior", "w_argprior", "w_dscp", "w_ll", "w_le", "w_ee", "w_sl", "w_se", "w_lerecall", "w_end", "w_sensim", "w_rel", "w_bias", "w_trans", "w_argtrans"}); //Method 9
        }

        public void destroyer()
        {
            /* Function Description: Destroys and clears the data structures */
            this.instructionPrior.Clear();
            this.verbCorrelation.Clear();
			this.predicateFreq.Clear ();
			this.predefinedTemplates.Clear ();
			this.gizaProbabilities.Clear ();
			this.transitionProbability.Clear ();
			this.argTransitionProbability.Clear ();
			this.endProbability.Clear ();
			this.relCorr.Clear ();
        }

        public void constructFeatureDataStructures(List<int> train, List<Boolean> methodFlag, List<VerbProgram> veil, 
		                                           List<Environment> envList, List<List<Environment>> listOfAllEnv, 
		                                           WordsMatching.SentenceSimilarity sensim)
        {
            /* Function Description : Given a training data initialize data structures which will be used to find
             * features during inference */
			if (train.Count () == 0)
				return;

			if (methodFlag [0]) 
			{
				this.buildInstructionPrior(train);         //build  instruction prior data
				this.buildObjectPrior(train);              //build object prior data
			}

			//if (methodFlag[3])
		    // this.buildBagOfWordRelation(train);

            if (methodFlag[1])
            	buildTemplate(); //build template for manual template baseline
		
			if (methodFlag [4]) 
			{
				this.ublSeedLexicons(listOfAllEnv, train, sensim);
				this.ublBaseLineBuildTrainFile (envList, train);
			}
		
			bool fullmodel = false;
			for (int i=5; i<methodFlag.Count(); i++) 
				fullmodel = fullmodel || methodFlag [i];

			if (fullmodel) 
			{
				this.buildTransitionStateProbabilities (listOfAllEnv, train);
				this.buildGizaProbabilities (train);
				this.buildEndStateProbabilities (envList, train);

				foreach (VerbProgram vp in veil) 
				{
					List<VeilTemplate> vtmps = vp.getProgram ();
					foreach (VeilTemplate vtmp in vtmps)
					{
						this.buildPredicateFrequency (vtmp);
						this.buildPredicateFrequencies(vtmp);
						this.buildRelCorrelation (vtmp);
					}
				}
			}

			//this.printFeatures ();
        }

		public void singletonUpdate(VeilTemplate vtmp)
		{
			/* Function Description: Given a new addition to VEIL templates, update the datastructures:
			 * Presently Updating: PredicateFrequency, RelCorrelation
			 * to do - TransitionStateProbabilities, EndStateProbabilities */

			this.buildPredicateFrequency (vtmp);
			this.buildRelCorrelation (vtmp);
		}

		public void printFeatures()
		{
			/*Function Description: Writes features in a file for analysis*/

			//print end-state feature
			System.IO.StreamWriter predfreq = new System.IO.StreamWriter (Constants.rootPath + "pred_frequency_feature.txt");
			predfreq.WriteLine ("Predicate Frequency Probability");
			foreach (KeyValuePair<String,int> freq in this.predicateFreq) 
				predfreq.WriteLine (freq.Key + " " + freq.Value.ToString ());
			predfreq.Flush ();
			predfreq.Close ();

			//print end-state feature
			System.IO.StreamWriter endstate = new System.IO.StreamWriter (Constants.rootPath + "end_state_feature.txt");
			endstate.WriteLine ("End State Probability");
			foreach (KeyValuePair<String,double> end in this.endProbability) 
				endstate.WriteLine (end.Key + " " + end.Value.ToString ());
			endstate.Flush ();
			endstate.Close ();

			//print rel-correlation feature
			System.IO.StreamWriter relcorel = new System.IO.StreamWriter (Constants.rootPath + "rel_correlation_feature.txt");
			relcorel.WriteLine ("Relation Correlation");
			foreach (Tuple<String,SpatialRelation,int> sp in this.relCorr) 
				relcorel.WriteLine (sp.Item1 + " " + sp.Item2.ToString () + " " + sp.Item3.ToString ());
			relcorel.Flush ();
			relcorel.Close ();

			//print transition probability
			System.IO.StreamWriter transition = new System.IO.StreamWriter (Constants.rootPath + "transition_probability.txt");
			transition.WriteLine ("Constraint1 \t Constraint2 \t Frequency");
			foreach (Tuple<String,String,double> trans in this.transitionProbability) 
				transition.WriteLine (trans.Item1+"\t"+trans.Item2+"\t"+trans.Item3);
			transition.Flush ();
			transition.Close ();

			//print transition probability
			System.IO.StreamWriter argTransition = new System.IO.StreamWriter (Constants.rootPath + "argtransition_probability.txt");
			argTransition.WriteLine ("Constraint1 \t Constraint2 \t Frequency");
			foreach (Tuple<String,String,double> trans in this.argTransitionProbability) 
				argTransition.WriteLine (trans.Item1+"\t"+trans.Item2+"\t"+trans.Item3);
			argTransition.Flush ();
			argTransition.Close ();
		}

		public Dictionary<String, double> getNullDictionary(int method)
		{
			/*Function Description: Creates a new dictionary of weights for the given method
			 with 0 weight values.*/
			Dictionary<String,double> weights = new Dictionary<String,double> ();
			for (int i=0; i<featureNames[method].Count(); i++) 
			{
				String featureName = featureNames [method] [i];
				int underscore = featureName.IndexOf('_');
				if (underscore != -1)
					featureName = featureName.Substring (underscore + 1);
				weights.Add (featureName, 0);
			}
			return weights;
		}

        public void buildTemplate()
        {
            /* Function Description: Parses the template file to bootstrap lexicons
             * verb-name1 variables  ->  logical-form
             * [relationship]r1 x1 y1; r2 x2 y2; ...
             * verb-name2 variables -> logical-form  etc. */

			String[] lines = System.IO.File.ReadAllLines(Constants.rootPath + @"Baselines/PredefinedTemplates/Templates.txt");
            for (int i = 0; i < lines.Count(); i=i+2)
            {
				Console.WriteLine ("Line "+i+" out of "+lines.Length);
                Template template = new Template();
				template.parse (lines [i], lines [i + 1]);
                this.predefinedTemplates.Add(template);
            }
        }

        /* Methods for computing Instruction Prior */
        public void buildInstructionPrior(List<int> train)
        {
            /* Function Description : Builds instruction prior table
             * ex: ("Grasp Cup",20) represents that it appeared 20 times */

            int numInst = 0;
            List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = this.obj.returnAllData();

            foreach (int point in train)
            {
                Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> info = datas[point];
                foreach (Instruction inst in info.Item3)
                {
                    bool added = false;
                    for (int i = 0; i < this.instructionPrior.Count(); i++)
                    {
                        Tuple<Instruction, double> insta = this.instructionPrior[i];
                        if (inst.compare(insta.Item1)) //earlier we were just seeing if name of the function is same
                        {
                            added = true;
                            this.instructionPrior[i] = new Tuple<Instruction, double>(insta.Item1, insta.Item2 + 1);
                        }
                    }
                    if (!added)
                        this.instructionPrior.Add(new Tuple<Instruction, double>(inst, 1));
                    numInst++;
                }
            }

            //normalize the instruction prior
            for (int i = 0; i < this.instructionPrior.Count(); i++)
            {
                Tuple<Instruction, double> insta = this.instructionPrior[i];
                this.instructionPrior[i] = new Tuple<Instruction, double>(insta.Item1, insta.Item2 / numInst);
            }
        }

        public double getInstructionPrior(Instruction inst)
        {
            /* Function Description : Return instruction prior of the given instruction */
            foreach (Tuple<Instruction, double> tmp in this.instructionPrior)
            {
                if (tmp.Item1.compare(inst))
                    return tmp.Item2;
            }
            return 0;
        }

        /* Method for computing Verb-Correlation Prior*/
        public void buildObjectPrior(List<int> train)
        {
            /*Function Description : Given an object as a descriptor in a particular point for
             * an instruction, it returns the frequency with which it has been in that place before*/

            List<Tuple<String, int, int>> countList = new List<Tuple<string, int, int>>();
            List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = this.obj.returnAllData();

            foreach (int entry in train)
            {
                Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> info = datas[entry];
                foreach (Instruction inst in info.Item3)
                {
                    String controllerFunction = inst.getControllerFunction();
                    List<String> args = inst.getDescription();
                    for (int padding = 0; padding < args.Count(); padding++) //for every description of s
                    {
                        bool added = false;
                        for (int iter = 0; iter < this.verbCorrelation.Count(); iter++)
                        {
                            Tuple<String, String, int, double> tmp = this.verbCorrelation[iter];
                            if (tmp.Item1.Equals(controllerFunction, StringComparison.OrdinalIgnoreCase)
                                && tmp.Item2.Equals(args[padding], StringComparison.OrdinalIgnoreCase)
                                && tmp.Item3 == padding)
                            {
                                this.verbCorrelation[iter] = new Tuple<string, string, int, double>(tmp.Item1, tmp.Item2, tmp.Item3, tmp.Item4 + 1);
                                Global.increment(countList, controllerFunction, padding);
                                added = true;
                            }
                        }
                        if (!added)
                        {
                            this.verbCorrelation.Add(new Tuple<string, string, int, double>(controllerFunction, args[padding], padding, 1));
                            Global.increment(countList, controllerFunction, padding);
                        }
                    }
                }
            }

            //normalize verb-correlation score
            for (int i = 0; i < this.verbCorrelation.Count(); i++)
            {
                Tuple<String, String, int, double> tmp = this.verbCorrelation[i];
                int counter = Global.getCount(countList, tmp.Item1, tmp.Item3);
                this.verbCorrelation[i] = new Tuple<String, String, int, double>(tmp.Item1, tmp.Item2, tmp.Item3, (tmp.Item4 / counter));
            }
        }

        public double getVerbCorrelation(String controllerFunction, String descp, int padding)
        {
            /*Function Description : Return instruction prior of the given instruction*/
            foreach (Tuple<String, String, int, double> tmp in this.verbCorrelation)
            {
                if (tmp.Item1.Equals(controllerFunction, StringComparison.OrdinalIgnoreCase)
                    && tmp.Item2.Equals(descp, StringComparison.OrdinalIgnoreCase)
                    && tmp.Item3 == padding)
                    return tmp.Item4;
            }
            return 0; //no correlation
        }

        public double getAvgVerbCorrelation(List<Instruction> inst)
        {
            /* Function Description: Find average verb correlation score for a sequence */
            int num = 0;
            double score = 0;
            for (int i = 0; i < inst.Count(); i++)
            {
                List<String> description = inst[i].getDescription();
                for (int j = 0; j < description.Count(); j++)
                {
                    score = score + this.getVerbCorrelation(inst[i].getControllerFunction(), description[j], j);
                    num++;
                }
            }
            return score / (num + Constants.epsilon);
        }

		public double giveInterpolationScore(List<Instruction> instructionSeq, Dictionary<String, Double> weights)
        {
            /* Function Description: Returns the score for the given instructinSeq - consists of
             * 1. Instruction Prior
             * 2. Verb Correlation Score
             * 3. Description Length */

            double instPrior = 0.0, vcorrScore = 0.0, descpL = 0.0;
            int numCorr = 0;

            foreach (Instruction inst in instructionSeq)
            {
				instPrior = instPrior + this.getInstructionPrior(inst);  //compute instruction prior
                //compute verb-correlation prior
                List<String> args = inst.getDescription();
                for (int i = 0; i < args.Count(); i++)
                {
                    vcorrScore = vcorrScore + this.getVerbCorrelation(inst.getControllerFunction(), args[i], i);
                    numCorr++;
                }
                //compute description length
                descpL = descpL + inst.norm();
            }

            if (instructionSeq.Count() == 0)
                instPrior = 0;
            else instPrior = instPrior / (double)(instructionSeq.Count());

            if (numCorr == 0)
                vcorrScore = 0;
            else vcorrScore = vcorrScore / (double)(numCorr);

			return weights["w_lip"] * instPrior + weights["w_lvcor"] * vcorrScore + weights["w_ldscp"] * descpL;
        }

		public double getAccumulatedScore(VeilTemplate vtmp, Clause cls, Environment envTest, List<Instruction> instruction,
		                                  List<Tuple<Object, String, String, int>> tableOfStates, int[] mapping, Dictionary<String,double> weights)
        {

            /*Function Description: Computes factor function Feature-Score 
             * 1.  Jump Operator Consistency Feature 
             * 2.  Object Distance
             * 3.  Intruction Prior
             * 4.  Description Length
             * 5.  Verb Correlation Score
             * 6.  Variability or Post Condition Satisfaction Score
             * 7.  Linguistic Similarity Score
             * 8.  Clausal Cover Score
             * 9.  Bag of Word */

            double objcorrelation = 0.0, instPrior = 0.0, descpL = 0.0, vcorrScore = 0.0, numCorr = 0, variability = 0.0, 
			       jumpScore = 0, linguisticScore = 0.0, cardinalityMatch = 0, bagOfWord = 0;

            /* Computing Variability Feature */
            //compute the last environment
			//Environment last = this.sml.executeList (instruction, envTest); 

            //compute average probability
            /*int counter = 0;
            foreach (Tuple<String, String> tmp in matching)
            {
                if (cl.ifExists(tmp.Item2)) //tmp.Item2 has to be from cl to continue
                {
                    Object obj1 = clEnv.findObject(tmp.Item2);
                    Object obj2 = last.findObject(tmp.Item1);
					//search for tmp.Item1 in last
                    /* iterate over states of obj and store it in the table if they are 
                       also states of obj1
                    List<Tuple<String, String>> newlyFoundStateList = obj2.getState();
                    foreach (Tuple<String, String> single in newlyFoundStateList)
                    {
                        if (obj1.getStateValue(single.Item1).Count() > 0)
                        {
                            //get the value of the [Obj1,single.Item1,single.Item2] from the table
                            variability = variability + getStateValueFrequency(tableOfStates, obj1.getName(), single.Item1, single.Item2);
                            counter++;
                        }
                    }
                }
            }
            if (counter > 0)
                variability = variability / counter;*/

			objcorrelation = this.objectCorrelation (vtmp, mapping, envTest);  //object correlation

            foreach (Instruction inst in instruction)
            {
				instPrior = instPrior + this.getInstructionPrior(inst); //compute instruction prior
                //compute verb-correlation prior
                List<String> args = inst.getDescription();
                for (int i = 0; i < args.Count(); i++)
                {
                    vcorrScore = vcorrScore + this.getVerbCorrelation(inst.getControllerFunction(), args[i], i);
                    numCorr++;
                }
				descpL = descpL + inst.norm(); //compute description length
            }

			instPrior = instPrior / (double)Math.Max (instruction.Count (), 1);
			vcorrScore = vcorrScore / (double)Math.Max (numCorr, 1);

            /*   Linguistic Similarity Score 
             *   Clause C = {c_1,c_2,... } and var = { x_i }_i
             *   define Matching M = [(x_i,y_i)] 
             *   Linguistic Score is defined as - 1/|M| sum_i H(x_i,y_i) */

			for (int i=0; i<mapping.Count(); i++) 
			{
				if (!Global.base_ (envTest.objects [mapping [i]].uniqueName).Equals (
					Global.base_ (vtmp.env_.objects [vtmp.xiOrigMappingInst [i]].uniqueName)))
					linguisticScore = linguisticScore + 1;
			}
            linguisticScore = linguisticScore / (double)Math.Max(mapping.Count(),1);

            //Clausal Score: Checks if the cardinality are same or not
			if (vtmp.cls_.lngObj.Count() != cls.lngObj.Count())
                cardinalityMatch = 1;

			Tuple<double, string> bagOfWordRes = Global.jaccardIndex(vtmp.cls_.lngObj.Select(x=>x.getName()).ToList(), 
			                                                         cls.lngObj.Select(x=>x.getName()).ToList());
			bagOfWordRes = new Tuple<double,String> (1-bagOfWordRes.Item1,bagOfWordRes.Item2);
            bagOfWord = bagOfWordRes.Item1;

            return weights["w_obj"] * objcorrelation + weights["w_ip"] * instPrior + weights["w_dscp"] * descpL + weights["w_vcor"] * vcorrScore +
                   weights["w_var"] * variability + weights["w_ling"] * linguisticScore + weights["w_card"] * cardinalityMatch
				   + weights["w_bog"] * bagOfWord; //every value is bounded by 0-1
        }

        public double objectCorrelation(VeilTemplate vtmp, int[] mapping, Environment envTest)
        {
            /* Function Description: EE correlation function. Given new mapping of variables, compute distance
             * between object that were used earlier and now */

			double totalScore = 0;

			for (int i=0; i<vtmp.zVariablesInst.Count(); i++) 
			{
				Object original = vtmp.env_.objects[vtmp.xiOrigMappingInst[i]];
				Object current = envTest.objects[mapping[i]];
				double score =  1 - original.findDistance(current.getState()).Item1;
				if (Global.base_ (original.uniqueName).Equals (Global.base_ (current.uniqueName)))
					score = 0.5 * score + 0.5;
				else
					score = 0.5 * score;

			}

			return totalScore/(double)Math.Max(vtmp.zVariablesInst.Count(),1);
        }

        public void buildBagOfWordRelation(List<int> trainData)
        {
            /* Function Description : Builds the bag of word relation */
            treeExp.Clear();
            dependent.Clear();
            #region commented
            /*List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = this.obj.returnAllData();
            foreach (int train in trainData)
            {
                Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> utmp = datas[train];

                foreach (Clause lng in utmp.Item4)
                {
                    List<String> clsW = new List<string>();
                    List<String> instW = new List<String>();
                    int start = lng.start_, end = lng.end_;

                    List<Clause> cls = lng.rootClause();
                    if (cls.Count() == 0)
                        continue;
                    clsW.Add(cls[0].returnVerb().getName());
                    List<SyntacticTree> sns = cls[0].returnNounList();
                    foreach (SyntacticTree sn in sns)
                        clsW.Add(sn.getName());

                    for (int i = start; i < end; i++)
                    {
                        Instruction inst = utmp.Item3[i];
                        instW.Add(inst.getControllerFunction());
                        foreach (String descp in inst.getDescription())
                            instW.Add(descp);
                    }

                    foreach (String w1 in clsW)
                    {
                        foreach (String w2 in instW)
                            this.incrementBOW(w1, w2);
                    }
                }

                for (int i = 0; i < utmp.Item3.Count() - 1; i++)
                    this.incrementDependent(utmp.Item3[i + 1], utmp.Item3[i]);
            }*/
            #endregion
        }

        public void incrementBOW(String clsW, String instW)
        {
            /* Function Description : Increment the count of this word */
            bool added = false;
            Tuple<String, String, int> res = null;
            foreach (Tuple<String, String, int> tmp in Features.treeExp)
            {
                if (tmp.Item1.Equals(clsW, StringComparison.OrdinalIgnoreCase) && tmp.Item2.Equals(instW, StringComparison.OrdinalIgnoreCase))
                {
                    res = tmp;
                    added = true;
                }
            }

            if (added)
            {
                Features.treeExp.Add(new Tuple<string, string, int>(clsW, instW, res.Item3 + 1));
                Features.treeExp.Remove(res);
            }
            else
            {
                Features.treeExp.Add(new Tuple<string, string, int>(clsW, instW, 1));
            }
        }

        public void incrementDependent(Instruction i1, Instruction i2)
        {
            /* Function Description : Increment the count of this word */
            bool added = false;
            Tuple<Instruction, Instruction, int> res = null;
            foreach (Tuple<Instruction, Instruction, int> tmp in Features.dependent)
            {
                if (tmp.Item1.compare(i1) && tmp.Item2.Equals(i2))
                {
                    res = tmp;
                    added = true;
                }
            }

            if (added)
            {
                Features.dependent.Add(new Tuple<Instruction, Instruction, int>(i1, i2, res.Item3 + 1));
                Features.dependent.Remove(res);
            }
            else
            {
                Features.dependent.Add(new Tuple<Instruction, Instruction, int>(i1, i2, 1));
            }
        }

        public double getStateValueFrequency(List<Tuple<Object, String, String, int>> tableName, String objName, String stateName, String valueName)
        {
            /* Function Description : For the table [a,b,c,d] return D/E where 
             *  D = { d | such that a.objName = objName, b=stateName, c=valueName } 
             *  E = | { a.objName = objName, b=stateName} |
             */
            double D = 0.0, E = 0.0;
            foreach (Tuple<Object, String, String, int> tmp in tableName)
            {
                if (tmp.Item1.getName().Equals(objName, StringComparison.OrdinalIgnoreCase) &&
                    tmp.Item2.Equals(stateName, StringComparison.OrdinalIgnoreCase))
                {
                    E = E + 1;
                    if (tmp.Item3.Equals(valueName, StringComparison.OrdinalIgnoreCase))
                    {
                        D = tmp.Item4;
                    }
                }
            }
            if (E == 0)
                return 0; //no evidence
            return D / E; //else return prior probability 
        }

		/*public double freqFeature(String constraints, VerbProgram v)
		{
			/* Function Description: Given a constraints in q1^q2^...q^k form
			 * return 1/k avg{freq(q_i,v)} */
		/*	String[] cstr = constraints.Split(new char[] { '^' });
			double score = 0;
			int z = v.totalFrequency ();
			for (int i=0; i<cstr.Length; i++) 
				score = score + v.fetchFrequency (cstr [i]);
			score = score / (cstr.Length*z + Constants.epsilon);
			return score;
		}*/

		private void buildPredicateFrequency(VeilTemplate vtmp)
		{
			/* Function Description: Given the veil library. Build a 
			 * table consisting of seen predicates along with their numbers.
			 * Since this is potentially exponential in number of objects 2^O(polynomial(objects)),
			 * we should try to get rid of this in the future. For now, its okay.
			 * This table can be used to judge the confidence of a predicate.
			 * e.g. whether (on coke_1 beer_1) makes sense or not. Technically it can but
			 * it may not appear in the training data at all hence of little semantic sense */

			foreach (String pred in vtmp.predicatesPostOld) 
			{
				this.zPredFreq++;
				if (!this.predicateFreq.ContainsKey (pred))
					this.predicateFreq.Add (pred, 1);
				else
					this.predicateFreq [pred]++;
			}
		}

		public int getPredicateFreq(String cstr)
		{
			//Function Desription: Returns the frequency of cstr
			if (this.predicateFreq.ContainsKey (cstr)) 
			{
				int val = this.predicateFreq [cstr];
				return val;
			}
			else
				return 0;
		}

		public void buildPredicateFrequencies(VeilTemplate vtmp)
		{ 
			/* Function Description: Builds predicate frequency table using groundings in veil template */

			for(int i=0; i<vtmp.predicatesPostOld.Count(); i++)
			{
				String predicate_ = vtmp.predicatesPostOld [i];
				if (!this.predicateFrequencies [0].ContainsKey (predicate_)) 
					this.predicateFrequencies [0] [predicate_] = 1;
				else this.predicateFrequencies [0] [predicate_]++;
			}

			for(int i=0; i<vtmp.predicatesPostOld.Count(); i++)
			{
				String predicate1_ = vtmp.predicatesPostOld [i];
				for(int j=i+1; j<vtmp.predicatesPostOld.Count(); j++)
				{
					String predicate2_ = vtmp.predicatesPostOld [j];
					List<String> atoms = new List<String>(){ predicate1_, predicate2_ };	
					atoms.Sort ();
					String postcondition_ = String.Join ("^",atoms);
					if (!this.predicateFrequencies [1].ContainsKey (postcondition_)) 
						this.predicateFrequencies [1] [postcondition_] = 1;
					else this.predicateFrequencies [1] [postcondition_]++;
				}
			}

			//---more general approach to be tried after the paper deadline
			/*for (int i=1; i<= Math.Min(this.maxPred,vtmp.predicatesPostOld.Count()); i++) 
			{
				int[] countingIndices = new int[i];
				for (int j=0; j<i; j++)
					countingIndices [j] = j; //thus starting configuration for countingIndices is 0,1,2,3...i-1

				bool allCombinationsNotCovered = true;
				int currentIndex = i-1;
				while(allCombinationsNotCovered)
				{
					String pred = ""; //generalize categories and arrange alphabetically
					if (!this.predicateFreq.ContainsKey (pred))
						this.predicateFreq.Add (pred, 1);
					else this.predicateFreq [pred]++;
					//update countingIndices

					if (countingIndices [currentIndex] < maxPred - 1) //space for incrementing
						countingIndices [currentIndex]++;
					else
					{
						if (currentIndex == 0)
							allCombinationsNotCovered = false;
						currentIndex--;
						countingIndices[currentIndex]++;
						for (int j=currentIndex+1; j<i; j++)
							countingIndices [j] = countingIndices [currentIndex] + j - currentIndex;
					}
				}
			}*/
		}

		public int getBaseFormPredicateFreq(String cstr)
		{
			/* Function Desription: Returns the base form frequency.
			 * E.g., (On Pillow_2 Loveseat_1) may not be seen but
			 * (On Pillow_1 Loveseat_1) might be. In which case, we can
			 * use the base-form frequency. Also (On $ Loveseat_1) will match anything at $ */

			int freq = 0;
			Tuple<bool,string> atom = Global.getAtomic (cstr);
			String[] words1 = atom.Item2.Split (new char[] {' '});
			for (int i=0; i<this.predicateFreq.Count(); i++) 
			{
				Tuple<bool,string> res = Global.getAtomic (this.predicateFreq.ElementAt(i).Key);
				bool same = true;
				if (atom.Item1 == res.Item1) 
				{
					String[] words2 = res.Item2.Split (new char[] { ' ' });
					if (words1.Length != words2.Length) 
						continue;
					for (int j=0; j<words1.Length; j++) 
					{
						if (!Global.base_ (words1 [j]).Equals (Global.base_ (words2 [j])) && 
						    !words1[j].Equals("$")) 
							same = false;
					}
				}
				else same = false;
				if (same)
					freq = freq + this.predicateFreq.ElementAt (i).Value;
			}
			return freq;
		}

		public void buildTransitionStateProbabilities(List<List<Environment>> listOfAllEnv, List<int> train)
		{
			/* Function Description: Builds transition probability as follows,
			 * for every point in the training data, we have a sequence of clause {C}
			 * for which we get a corresponding sequence of states {p1}-{p2}...{pk}
			 * we then create transition probability using P({pi}|{pi-1}) for every i
			 * we then split it to P({pi}|{pi-1}) = product_{jl} P({pi}_j | {pi-1}_l) */

			List<Tuple<Tuple<int,int>, Clause, List<Instruction>, List<Clause>>> datas = this.obj.returnAllData ();
			List<List<Tuple<int,int>>> alignment = this.obj.returnAllAlignment ();
			Dictionary<String,int> normalizer = new Dictionary<string, int> ();
			Dictionary<String,int> argNormalizer = new Dictionary<string, int> ();

			foreach(int index in train)
			{
				/* for every consecutive clause Ci, Ci+1; find the corresponding
				 * difference predicates from their starting environment. i.e., if the environment at the beginning
				 * for Ci was Ei and that for Ci+1 was Ei+1 and if the environment at the end of Ci+1 was Ei+2 then
				 * find the difference between Ei+1 and Ei and Ei+2 and Ei+1. */

				for(int clsNo=0; clsNo<datas [index].Item4.Count()-1; clsNo++)
				{
					Tuple<int,int> align1 = alignment [index] [clsNo];
					Tuple<int, int> align2 = alignment [index] [clsNo + 1];
					//if any one of the alignment is empty then continue
					if (align1.Item1 > align1.Item2 || align2.Item1 > align2.Item2)
						continue;

					Environment ei = listOfAllEnv[index][align1.Item1];
					Environment ei1 = listOfAllEnv[index][align2.Item1];
					Environment ei2 = listOfAllEnv[index][align2.Item2+1];

					List<String> constraint1 = ei1.difference (ei);
					List<String> constraint2 = ei2.difference (ei1);

					//create probability 
					foreach (String cstr1 in constraint1) 
					{
						if (normalizer.ContainsKey (cstr1))
							normalizer [cstr1] = normalizer [cstr1] + constraint2.Count ();
						else normalizer.Add (cstr1, constraint2.Count ());

						foreach (String cstr2 in constraint2) 
						{
							int elemIndex = this.transitionProbability.FindIndex (x => x.Item1.Equals (cstr1) && x.Item2.Equals (cstr2));
							if (elemIndex == -1) 
								this.transitionProbability.Add (new Tuple<string, string, double> (cstr1, cstr2, 1));
							else
								this.transitionProbability [elemIndex] = new Tuple<string, string, double> (cstr1, cstr2, this.transitionProbability[elemIndex].Item3+1);

							List<String> argCstr =  Global.parameterize(new List<String>(){cstr1, cstr2,});

							if (argNormalizer.ContainsKey (argCstr [0]))
								argNormalizer [argCstr [0]] = argNormalizer [argCstr [0]] + 1;
							else argNormalizer.Add (argCstr[0], 1);

							int argElemIndex = this.transitionProbability.FindIndex (x => x.Item1.Equals (argCstr[0]) && x.Item2.Equals (argCstr[1]));
							if (argElemIndex == -1) 
								this.argTransitionProbability.Add (new Tuple<string, string, double> (argCstr[0], argCstr[1], 1));
							else
								this.argTransitionProbability [argElemIndex] = new Tuple<string, string, double> (argCstr[0], argCstr[1], this.argTransitionProbability[argElemIndex].Item3+1);
						}
					}
				}
			}

			#region normalize_the_scores
			for(int i=0; i<this.transitionProbability.Count();i++)
			{
				Tuple<String,String,double> unnormalizedVal_ = this.transitionProbability[i];
				this.transitionProbability[i] = new Tuple<string, string, double>(unnormalizedVal_.Item1,unnormalizedVal_.Item2,unnormalizedVal_.Item3/(double)normalizer[unnormalizedVal_.Item1]);
			}
			//I should do Laplace smoothing in future as well

			for(int i=0; i<this.argTransitionProbability.Count();i++)
			{
				Tuple<String,String,double> unnormalizedVal_ = this.argTransitionProbability[i];
				this.argTransitionProbability[i] = new Tuple<string, string, double>(unnormalizedVal_.Item1,unnormalizedVal_.Item2,unnormalizedVal_.Item3/(double)argNormalizer[unnormalizedVal_.Item1]);
			}
			#endregion
		}

		public double fetchTransitionProbability(String constraint1, String constraint2)
		{
			// Function Description: Fetch transition probability P(constraint1|constraint2)
			if (constraint1 == null || constraint2 == null)
				return 0;
			String[] cstr1 = constraint1.Split (new char[] {'^'}).ToArray();
			String[] cstr2 = constraint2.Split (new char[] {'^'}).ToArray();

			if (cstr1.Length == 0 || cstr2.Length == 0)
				return 0;

			//P(cstr1|cstr2) = Prod_{ij} p(cstr1_i | cstr2_j)
			double average = 0;
			foreach (String cstr1_ in cstr1) 
			{
				foreach (String cstr2_ in cstr2) 
				{
					int index = this.transitionProbability.FindIndex (x => x.Item1.Equals (cstr1_) && x.Item2.Equals (cstr2_));
					if (index == -1)
						average = average + 0; //0 is placeholder, should return Laplace Smoothing
					else average = average + this.transitionProbability [index].Item3;
				}
			}
			return average / (double)cstr1.Count () * cstr2.Count ();
		}

		public double fetchArgTransitionProbability(String constraint1, String constraint2)
		{
			// Function Description: Fetch transition probability P(constraint1|constraint2)
			if (constraint1 == null || constraint2 == null)
				return 0;

			String[] cstr1 = constraint1.Split (new char[] {'^'}).ToArray();
			String[] cstr2 = constraint2.Split (new char[] {'^'}).ToArray();

			if (cstr1.Length == 0 || cstr2.Length == 0)
				return 0;

			//P(cstr1|cstr2) = Prod_{ij} p(cstr1_i | cstr2_j)
			double average = 0;
			foreach (String cstr1_ in cstr1) 
			{
				foreach (String cstr2_ in cstr2) 
				{
					List<String> argcstr = Global.parameterize (new List<String> { cstr1_, cstr2_ });
					int index = this.argTransitionProbability.FindIndex (x => x.Item1.Equals (argcstr[0]) && x.Item2.Equals (argcstr[1]));
					if (index == -1)
						average = average + 0; //0 is placeholder, should return Laplace Smoothing
					else average = average + this.argTransitionProbability [index].Item3;
				}
			}
			return average / (double)cstr1.Count () * cstr2.Count ();
		}


		public void buildEndStateProbabilities(List<Environment> envList, List<int> train)
		{
			/* Function Description: Given the data, build a table P
			 * e.g., P(In(x,y)) gives the probability of an object
			 * being inside another object. Can be used to show that
			 * e.g., P(Grasping(x,y)) never occurs or is too unlikely. */

			List<Tuple<Tuple<int,int>, Clause, List<Instruction>, List<Clause>>> datas = this.obj.returnAllData ();

			foreach(int index in train)
			{
				Tuple<Tuple<int,int>, Clause, List<Instruction>, List<Clause>>  data = datas [index];
				Environment final = this.sml.executeList (data.Item3, envList [data.Item1.Item1 - 1]);
				List<String> difference = final.difference (envList [data.Item1.Item1 - 1]);
				foreach(String predicate in difference)
				{
					/* Given a predicate such as (not (state Cup1 IceCream)) we generalize
					 * it in two ways by replacing only the objects and then by replacing 
					 * both the objects and the state */
					Tuple<bool, string> atom = Global.getAtomic(predicate);
					String[] words = atom.Item2.Split (new char[] {' '});
					this.zEndProb++;
					if (words [0].Equals ("state"))
					{
						String t1 = "state "+words[1]+" y";
						String t2 = "state x y";

						if (atom.Item1) 
						{
							t1 = "not ("+t1+")";
							t2 = "not ("+t2+")";
						}

						/*if (this.endProbability.ContainsKey (t1))
							this.endProbability [t1] = this.endProbability [t1] + 1;
						else
							this.endProbability.Add (t1, 1);*/

						if (this.endProbability.ContainsKey (t2))
							this.endProbability [t2] = this.endProbability [t2] + 1;
						else
							this.endProbability.Add (t2, 1);
					} 
					else
					{
						String t = words[0]+" x y";
						if (atom.Item1) 
							t = "not ("+t+")";

						if (this.endProbability.ContainsKey (t))
							this.endProbability [t] = this.endProbability [t] + 1;
						else
							this.endProbability.Add (t, 1);
					}

					/*if (this.endProbability.ContainsKey (predicate.Substring (1, predicate.Length - 2)))
						this.endProbability [predicate.Substring (1, predicate.Length - 2)] = this.endProbability [predicate.Substring (1, predicate.Length - 2)] + 1;
					else
						this.endProbability.Add (predicate.Substring (1, predicate.Length - 2), 1);*/
				}
			}
			//Dump it in a file for debugging
			System.IO.StreamWriter swf = new System.IO.StreamWriter(Constants.rootPath+"endStateProb_debug.txt");
			foreach (KeyValuePair<string, double> entry in this.endProbability) 
				swf.WriteLine (entry.Key + " : " + entry.Value / (zEndProb + Constants.epsilon));
			List<String> keys = new List<string>(this.endProbability.Keys);
			double sum = 0;
			foreach (String key in keys) 
			{
				this.endProbability [key] = this.endProbability [key] / (zEndProb + Constants.epsilon);
				sum = sum + this.endProbability [key];
			}

			foreach (String key in keys) //to increase their weightage
				this.endProbability [key] = this.endProbability [key] / sum;

			swf.Flush ();
			swf.Close ();
		}

		public double getEndStateProbability(String constraint)
		{
			/* Function Description: Given a constraint p1^p2^...pk
			 * returns average(Probability(pi))/k */

			String[] cstr = constraint.Split (new char[] {'^' });
			double endState = 0;
			for (int i=0; i<cstr.Length; i++) 
			{ 
				Tuple<bool, string> atom = Global.getAtomic(cstr[i]);
				String[] words = atom.Item2.Split (new char[] {' '});
				if (words [0].Equals ("state")) 
				{
					String t = "state x y";
					if (atom.Item1)
						t = "not (" + t + ")";
					if (this.endProbability.ContainsKey (t))
						endState = endState + this.endProbability [t];
				} 
				else 
				{
					String t = words [0] + " x y";
					if (atom.Item1)
						t = "not (" + t + ")";
					if (this.endProbability.ContainsKey (t))
						endState = endState + this.endProbability [t];
				}
			}

			return endState / (cstr.Length + Constants.epsilon);
		}

		public void buildGizaProbabilities(List<int> train)
		{
			/* Function Description: Builds Giza probability tables for */

			#region create_the_files
			List<Tuple<Tuple<int,int>, Clause, List<Instruction>, List<Clause>>> datas = this.obj.returnAllData ();
			List<List<Tuple<int,int>>> alignment = this.obj.returnAllAlignment ();
			System.IO.StreamWriter english = new System.IO.StreamWriter (Constants.rootPath+@"Alignments/english.txt");
			System.IO.StreamWriter code = new System.IO.StreamWriter (Constants.rootPath+@"Alignments/code.txt");
			foreach(int tr in train)
			{
				for (int cls = 0; cls < datas[tr].Item4.Count(); cls++) 
				{
					String sentence = datas [tr].Item4 [cls].sentence;
					if (sentence == null || alignment[tr][cls].Item1 > alignment[tr][cls].Item2)
						continue;

					sentence = sentence.Replace("\n","").Trim();
					english.WriteLine (sentence.ToLower());

					List<String> objNames = new List<String> ();
					for (int iter = alignment[tr][cls].Item1; iter <= alignment[tr][cls].Item2; iter++) 
						objNames = objNames.Union(datas [tr].Item3 [iter].returnObject ()).ToList();
					objNames = objNames.Select (x => x.ToLower ()).ToList();

					code.WriteLine (String.Join (" ", objNames));
				}
			}
			english.Flush ();
			english.Close ();
			code.Flush (); 
			code.Close ();
			#endregion

			#region call_the_gizapp file
			Process proc = new Process();
			proc.StartInfo.WorkingDirectory = Constants.rootPath + @"Alignments/";
			proc.StartInfo.FileName = "/bin/sh";//Constants.rootPath + @"Alignments/createGizaFiles.sh";
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.Arguments = string.Format(Constants.rootPath + @"Alignments/createGizaFiles.sh");
			proc.Start();
			proc.WaitForExit();
			#endregion

			#region generate_the_tables
			//Read the code file
			Dictionary<int,string> engVocab = new Dictionary<int,string>();
			Dictionary<int,string> codeVocab = new Dictionary<int,string>();

			engVocab.Add(0,"Null");
			codeVocab.Add(0,"Null");
			String[] engVocabLines = System.IO.File.ReadAllLines(Constants.rootPath+ @"Alignments/english.vcb");
			for(int i=0; i<engVocabLines.Length;i++)
			{
				String[] words = engVocabLines[i].Split(new char[]{' '}).Select(x=>x.Trim()).ToArray();
				engVocab.Add(Int32.Parse(words[0]),words[1]);
			}

			String[] codeVocabLines = System.IO.File.ReadAllLines(Constants.rootPath+ @"Alignments/code.vcb");
			for(int i=0; i<codeVocabLines.Length;i++)
			{
				String[] words = codeVocabLines[i].Split(new char[]{' '}).Select(x=>x.Trim()).ToArray();
				codeVocab.Add(Int32.Parse(words[0]),words[1]);
			}

			//Read the file ending with t3.final
			String[] files = System.IO.Directory.GetFiles(Constants.rootPath+@"Alignments/", "*.t3.final");
			if(files.Length!=1)
				throw new ApplicationException("There should be exactly one t3.final file");

			String[] values = System.IO.File.ReadAllLines(files[0]);
			for(int i=0; i<values.Length;i++)
			{
				String[] words = values[i].Split(new char[]{' '}).Select(x=>x.Trim()).ToArray();
				double probability = Double.Parse(words[2]);
				if(probability < Constants.epsilon)
					continue;
				this.gizaProbabilities.Add(new Tuple<string, string, double>(engVocab[Int32.Parse(words[0])],
				                           codeVocab[Int32.Parse(words[1])], probability));
			}

			//Delete all file except createGizeFile.sh
			String[] allfile = System.IO.Directory.GetFiles(Constants.rootPath+@"Alignments/");
			for(int tmp = 0; tmp<allfile.Length;tmp++)
			{
				if(allfile[tmp].Equals(Constants.rootPath+@"Alignments/createGizaFiles.sh"))
					continue;
				System.IO.File.Delete(allfile[tmp]);
			}
			#endregion
		}

		public void buildRelCorrelation(VeilTemplate vtmp)
		{
			/* Function Description: Given training data, build correlation
			 * between relationship and spatial relation. E.g., if the command is
			 * keep the cup on the table, and the corresponding end state has On(cup,table)
			 * then add +1 for On, on */

			List<String> rel = new List<string> (); //list of relations
			List<SpatialRelation> sp = new List<SpatialRelation> (); //list of spatial relations 

			//find all relations in the clause of vt
			String[,] relMatrix = vtmp.cls_.relation;
			if (relMatrix == null)
				return;

			for (int i=0; i<relMatrix.GetLength(0); i++) 
			{
				for (int j=0; j<relMatrix.GetLength(1); j++) 
				{
					if (relMatrix [i, j]!=null && !relMatrix [i, j].Equals ("None")) 
						rel.Add (relMatrix[i,j]);
				}
			}

			//find all spatial relations in the end-state predicate of vt
			foreach (String predicate in vtmp.predicatesPost) 
			{
				Tuple<bool,String> atom = Global.getAtomic (predicate);
				String[] words = atom.Item2.Split (new char[] { ' ' });
				if (atom.Item1 && !words[0].Equals("state")) 
					sp.Add (Environment.parseRelationship (words[0]));
			}

			foreach(String rel_ in rel)
			{
				foreach (SpatialRelation sp_ in sp) 
				{
					int index = this.relCorr.FindIndex(x=>x.Item1.Equals(rel_,StringComparison.OrdinalIgnoreCase)
					                                                              && x.Item2==sp_);
					if (index != -1) 
					{
						Tuple<String,SpatialRelation,int> old = this.relCorr [index];
						this.relCorr [index] = new Tuple<string, SpatialRelation, int> (old.Item1, old.Item2, old.Item3+1);
					}
					else this.relCorr.Add (new Tuple<string, SpatialRelation, int>(rel_,sp_,1));
				}
			}
		}

		public Tuple<double,string> fetchRelationshipFeature(Clause cls, List<List<String>> plurality, String constraint)
		{
			/* Function Description: If the algorithm contains relations e.g., On
			 * then we will like to see more relationship in the constraint. Thus, we
			 * add the relationship feature computed as follows:- 
             * M1. if no relation then return 1
             * else if there is a relation, return 1 if there exists a relationship type constraint In or On; else return 0. 
             * M2. for each relation rel between x and y; if x and y have valid mapping; add +1 if there exist sp(x,y) else 0 
             *     thus, put game into the xbox -- we get +1 for (In CD_1 Xbox_1) since game->cd_1 and xbox->xbox_1 */

			List<String> predicates = constraint.Split (new char[] { '^' }).ToList ();
			List<String> handling = new List<string> () {"inside", "in", "into", "on", "onto" };
			//find all relations in the clause of vt
			String[,] relMatrix = cls.relation;
			if (relMatrix == null)
				return new Tuple<double, String>(1,"");

			String log = "";

			double score = 0;
			int count = 0;
			for (int i=0; i<relMatrix.GetLength(0); i++) 
			{
				for (int j=0; j<relMatrix.GetLength(1); j++) 
				{
					//check if i and j have non-zero mapping
					if (plurality [i].Count () == 0 || plurality [j].Count () == 0) 
						continue;

					if (relMatrix [i, j] != null && !relMatrix [i, j].Equals ("NONE", StringComparison.OrdinalIgnoreCase)) 
					{
						String relationship = relMatrix [i, j].ToLower();
						if (!handling.Contains (relationship)) 
							continue;

						bool found = false;
						//give a score of 1, if there exist a predicate (rel x y) such that x, y are mapping of i,j resp.
						foreach (String predicate in predicates)
						{
							Tuple<bool, String> base_ = Global.getAtomic (predicate);
							if (base_.Item1) //we want a positive relationship
								continue;
							String[] words = base_.Item2.Split (new char[] { ' ' });
							if (!words[0].Equals("state") && plurality[i].Contains(words[1]) && plurality[j].Contains(words[2])) 
							{
								found = true;
								log = log + " " + relationship + " maps to " + predicate;
								break;
							}
						}

						if (found) //if found then give a score of +1
							score = score + 1;
						else
							log = log + " " + relationship + " no map";
						count++;
					}
				}
			}

			if (count == 0)
				return new Tuple<double, String>(1,"");
			return new Tuple<double, String>(score/(double)count, log);
		}


		public void ublSeedLexicons(List<List<Environment>> listOfAllEnv, List<int> train, WordsMatching.SentenceSimilarity sensim)
		{
			/* Function Description: Generates seed lexicons for the UBL algorithm 
			 * for each clause in train and for each environment. The algorithm
			 * uses the LECorrelation Matrix and the maps w to obj such that obj = argmax_{obj_i} LE[w,obj_i] */

			System.IO.StreamWriter seedLexFile = new System.IO.StreamWriter (Constants.ublPath + "../../../../../../en-np-fixedlex.geo");
			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = this.obj.returnAllData();
			List<List<Tuple<int, int>>> alignment = this.obj.returnAllAlignment();

			foreach(int pt in train)
			{
				//Environment start = envList[datas[pt].Item1.Item1-1];
				Console.WriteLine ("Reached Environment " + listOfAllEnv [pt].Count () + " vs instruction " + datas [pt].Item3.Count ());
				Console.WriteLine("Alignment "+datas[pt].Item4.Count()+" vs "+alignment[pt].Count());

				for(int i=0; i<datas[pt].Item4.Count(); i++)
				{
					if (alignment [pt] [i].Item2 < alignment [pt] [i].Item1)
						continue;
					Clause cls = datas [pt].Item4 [i];
					Console.WriteLine ("Alignment instance  " + alignment [pt] [i].Item1);
					Environment env = listOfAllEnv [pt] [alignment [pt] [i].Item1];
					double[,] leCorrMatrix = env.getLECorrMatrix(cls, datas[pt].Item3.GetRange(0,alignment [pt] [i].Item1),
					                                             sensim, this);
					for (int lang=0; lang<leCorrMatrix.GetLength(0); lang++) 
					{
						int maxIndex = 0;
						for (int obj=0; obj<leCorrMatrix.GetLength(1); obj++) 
						{
							if (leCorrMatrix [lang, obj] > leCorrMatrix [lang, maxIndex])
								maxIndex = obj;
						}

						//for each object with the same value, add the maxIndex
						for (int obj = 0; obj<leCorrMatrix.GetLength(1); obj++) 
						{
							if (leCorrMatrix [lang, obj] == leCorrMatrix [lang, maxIndex])
								seedLexFile.WriteLine (cls.lngObj[lang].getName()+" :- NP : "+env.objects[obj].uniqueName+":o");
						}
					}
				}
			}

			seedLexFile.Flush ();
			seedLexFile.Close ();
		}

		public void ublBaseLineBuildTrainFile(List<Environment> envList, List<int> train)
		{
			/* Function Description: Builds the training file needed by the ubl baseline  */

			#region create_train_and_test_file
			System.IO.StreamWriter trainFile = new System.IO.StreamWriter(Constants.ublPath+"train");

			System.IO.StreamWriter gizaLangFile = new System.IO.StreamWriter(Constants.gizaPath+"english.txt");
			System.IO.StreamWriter gizaMRFile = new System.IO.StreamWriter(Constants.gizaPath+"lambda.txt");

			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = this.obj.returnAllData();
			List<List<Tuple<int, int>>> alignment = this.obj.returnAllAlignment();

			foreach(int pt in train)
			{
				Environment start = envList[datas[pt].Item1.Item1-1];
				for(int i=0; i<datas[pt].Item4.Count(); i++)
				{
					String sen = datas[pt].Item4[i].sentence.Replace('\n',' ').ToLower();
					int startIndex = alignment[pt][i].Item1, endIndex = alignment[pt][i].Item2;
					if(startIndex > endIndex)
						continue;
					Environment end = this.sml.executeList(datas[pt].Item3.GetRange(startIndex, endIndex-startIndex+1), start);
					List<String> difference = Global.reformatStatePredicatesForGiza(VeilTemplate.takeOnlyPositivePredicates(end.difference(start)));
					if(difference.Count() == 0)
						continue;
					start = end;

					trainFile.WriteLine(sen);
					trainFile.WriteLine("(and "+String.Join(" ",difference)+")\n"); //grounding is the conjunction of end-state e.g., get me a cup -> (Grasping Robot cup)
					gizaLangFile.WriteLine(sen);
					gizaMRFile.WriteLine("(and "+String.Join(" ",difference)+")");
				}
			}

			trainFile.Flush();
			trainFile.Close();
			gizaLangFile.Flush();
			gizaLangFile.Close();
			gizaMRFile.Flush();
			gizaMRFile.Close();
			#endregion

			#region call_the_gizapp file
			List<String> possibleST = new List<String>(){"w-c","c-w"};
			Process proc = new Process();

			foreach(String st in possibleST)
			{
				proc.StartInfo.WorkingDirectory = Constants.rootPath + @"Baselines/UBL";
				proc.StartInfo.FileName = "/bin/sh";
				proc.StartInfo.RedirectStandardError = true;
				proc.StartInfo.RedirectStandardOutput = true;
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.CreateNoWindow = true;
				proc.StartInfo.Arguments = string.Format(Constants.rootPath + @"Baselines/UBL/"+st+"GizaFiles.sh");
				proc.Start();
				proc.WaitForExit();

				//read the t3 final and save them
				String[] files = System.IO.Directory.GetFiles(Constants.rootPath+@"Baselines/UBL/", "*.t3.final");
				Console.WriteLine("st = "+st+" Num files "+files.Length);
				if(files.Length!=1)
					throw new ApplicationException("There should be exactly one t3.final file");

				String[] uncodified = System.IO.File.ReadAllLines(files[0]);
				Dictionary<int,String> vocabEnglish = Global.vocabToDictionary(System.IO.File.ReadAllLines(Constants.gizaPath+"english.vcb"));
				Dictionary<int,String> vocabLambda = Global.vocabToDictionary(System.IO.File.ReadAllLines(Constants.gizaPath+"lambda.vcb"));
				String[] decoded = null;

				if(st.Equals("w-c"))
					decoded = Global.renumerateGizaFiles(uncodified, vocabEnglish, vocabLambda);
				else decoded = Global.renumerateGizaFiles(uncodified, vocabLambda, vocabEnglish);

				System.IO.StreamWriter sw = new System.IO.StreamWriter(Constants.ublPath+st+".giza_probs");
				for(int i=0; i<decoded.Length; i++)
					sw.WriteLine(decoded[i]);
				sw.Flush();
				sw.Close();

				//Delete all non-script files
				String[] allfile = System.IO.Directory.GetFiles(Constants.rootPath+@"Baselines/UBL/");
				for(int tmp = 0; tmp<allfile.Length;tmp++)
				{
					if(allfile[tmp].EndsWith(".sh"))
						continue;
					System.IO.File.Delete(allfile[tmp]);
				}
			}
			#endregion
		}
    }
}
