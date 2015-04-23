/* Tell Me Dave 2013-14, Robot-Language Learning Project
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
    class Robot
    {
        /*  Class Description : Represents the robot in an environment. 
         *  At this point, this class is a heavily compromised version of
         *  what a robot should be */

        private Tuple<double, double> location = null;
        private Object hand1 = null; //null means no object grasped
        private Object hand2 = null; //null means no object grasped
        private List<Object> sensed = new List<Object>(); //List of Objects that the robot has already sensed

        public Tuple<double, double> location_
        {
            get { return location; }
        }

        public void copyRobot(Robot copy, Environment env)
        {
            /*Function Description : Creates of copy of this in copy*/
            if (this.hand1 != null)
                copy.hand1 = env.findObject(this.hand1.uniqueName); //wrong
            else copy.hand1 = null;
            if (this.hand2 != null)
                copy.hand2 = env.findObject(this.hand2.uniqueName); //wrong
            else copy.hand2 = null;
            copy.location = new Tuple<double, double>(location.Item1, location.Item2);
        }

        public void display(Logger lg)
        {
            /*Function Description : Display the robot*/
            lg.writeToFile("<div id='robot'> Robot State : ");
            if (this.hand1 != null)
                lg.writeToFile("Hand-1 : Grasping " + this.hand1.getName() + " ");
            else lg.writeToFile("Hand-1 : Free ");
            if (this.hand2 != null)
                lg.writeToFile("Hand-2 : Grasping : " + this.hand2.getName() + " ");
            else lg.writeToFile("Hand-2 : Free ");
            lg.writeToFile("</div>");
        }

        public bool armsFree()
        {
            /* Function Description : Returns true if atleast one arm is free*/
            if (hand1 == null || hand2 == null)
            {
                return true;
            }
            return false;
        }

        public bool isGrasping(String objName)
        {
            /* Function Description : True if the robot is grasping this object*/
            if (hand1 != null && hand1.getName().Equals(objName, StringComparison.OrdinalIgnoreCase))
                return true;
            if (hand2 != null && hand2.getName().Equals(objName, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
        
        public void moveRobot(double x,double y)
        {
            /* Function Description : Move the robot from one to different location*/
            location = new Tuple<double, double>(x,y);
        }

        public bool isNear(Object obj)
        {
            /* Function Description : Returns true if the robot is 
             * near the given object */
            if (this.isGrasping(obj.uniqueName))
                return true;
            Tuple<int, int> pos = obj.returnObjectGroundLoc();
            if (this.location.Item1 == pos.Item1 && this.location.Item2 == pos.Item2)
                return true;
            else return false;
        }

        public void grasp(Object obj)
        {
            /*Function Description : Make the robot grasp the object. For it the object
             * must not already be grasped, it must be in neighborhood of arm and one of its
             * arm must be free*/
            if (hand1 != null && hand1.uniqueName.Equals(obj.uniqueName, StringComparison.OrdinalIgnoreCase) || hand2!=null && hand2.uniqueName.Equals(obj.uniqueName, StringComparison.OrdinalIgnoreCase))
            {
                //already grasped - do nothing or throw Warning
                return;
            }

            if(hand1==null)
            {
                hand1 = obj;
            }
            else if(hand2==null)
            {
                hand2 = obj;
            }
            else
            {
                //raise execption - Cannot grasp Object
            }
        }

        public void release(Object obj)
        {
            /*Function Description : Release a grasped object. It just retracts its arm back.*/
            if (hand1 != null && hand1.uniqueName.Equals(obj.uniqueName,StringComparison.OrdinalIgnoreCase))
            {
                hand1 = null;
            }
            else if (hand2 !=null && hand2.uniqueName.Equals(obj.uniqueName, StringComparison.OrdinalIgnoreCase))
            {
                hand2 = null;
            }
        }

        public Object returnLeftHandObject()
        {
            /* Function Description : Return the object 
             * being held by the left hand */
            return this.hand1;
        }

        public Object returnRightHandObject()
        {
            /* Function Description : Return the object 
             * being held by the right hand */
            return this.hand2;
        }

    }
}
