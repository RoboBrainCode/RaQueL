﻿/* Tell Me Dave 2013-14, Robot-Language Learning Project
 * Code developed by - Dipendra Misra (dkm@cs.cornell.edu)
 * working in Cornell Personal Robotics Lab.
 * 
 * More details - http://tellmedave.cs.cornell.edu
 * This is Version 2.0 - it supports data version 1.1, 1.2, 1.3
 */

/*  Notes for future Developers - 
 *    <no - note >
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompton
{
    class Processing
    {
        /*Class Description : Defines functions for language processing*/

        public static void stemming(SyntacticTree verb)
        {
            /*Function Description: Stems the verb*/
            String verbName = verb.getName();
            int length = verbName.Count();

            if (verbName.EndsWith("ed"))
            {
                verbName=verbName.Substring(0, length - 2);
            }
            else if (verbName.EndsWith("ing"))
            {
                verbName = verbName.Substring(0, length - 3);
            }
            else if (verbName.EndsWith("e"))
            {
                verbName = verbName.Substring(0, length - 1);
            }

            verb.changeVerb(verbName);
        }

		public static string basePlural(String words)
		{
			/* Function Description: Given a word such as Pillows, cup, trees etc. 
			 * it returns the singular form such as pillow, cup etc. if its a numerical type of noun 
			 * currently assuming this and simply trimming the ending s letter */
			if (words.Length > 0 && words [words.Length - 1] == 's') 
				return words.Substring (0, words.Length - 1);
			return words;
		}

		public static bool isPlural(String word)
		{
			/*Function Description: Given a word returns True if its plural else False
			 e.g., keeps, mugs, cups are plural while keep, mug and cup are singular 
			 Currently using a simple hack of letters ending with s*/
			if (word.Length == 0)
				throw new ApplicationException ("Word is empty; expecting a name as cup, keeps, tree, etc. ");

			if (word [word.Length - 1] == 's')
				return true;
			return false;
		}
    }
}
