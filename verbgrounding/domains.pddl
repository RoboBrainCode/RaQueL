 (define (problem MakingRamen50)
(:objects Cup Mug Table Desk Microwave Plate Tap Stove Stoveknob Door Button Coffee-Powder Milk-Box Sugar-Box
 microwave_1 crockPot_1 fridge_1 syrupBottle_1 ramen_1 spoon_1 iceCreamBox_1 counter_1 sink_1 
stove_1Knob_1 stove_1Knob_2 stove_1Knob_3 stove_1Knob_4 sink_1Knob_1 
stove_1Burner_1 stove_1Burner_2 stove_1Burner_3 stove_1Burner_4)
(:init (IsOpenable Door)
(IsOpenable microwave_1)
(IsFindable Cup)
(IsFindable Mug)
(IsFindable Table)
(IsFindable Desk)
(IsFindable Stove)
(IsFindable StoveKnob)
(IsFindable Microwave)
(IsFindable Door)
(IsFindable Coffee-Powder)
(IsFindable Tap)
(IsFindable Milk-Box)
(IsFindable Sugar-Box)
(IsFindable Plate)
(IsFindable mug_1)
(IsFindable stove_1)
(IsFindable microwave_1)
(IsFindable crockPot_1)
(IsFindable fridge_1)
(IsFindable syrupBottle_1)
(IsFindable ramen_1)
(IsFindable spoon_1)
(IsFindable iceCreamBox_1)
(IsFindable counter_1)
(IsFindable sink_1)
(IsGraspable Mug)
(IsGraspable Cup)
(IsGraspable Tap)
(IsGraspable Coffee-Powder)
(IsGraspable Milk-Box)
(IsGraspable Sugar-Box)
(IsGraspable Plate)
(IsGraspable Mug1)
(IsGraspable crockPot_1)
(IsGraspable syrupBottle_1)
(IsGraspable ramen_1)
(IsGraspable spoon_1)
(IsGraspable iceCreamBox_1)
(IsPressable Button On)
(IsPressable fridge_1WaterButton On)
(IsPressable microwave_1CookButton On)
(IsTurnable Tap)
(IsTurnable StoveKnob)
(IsTurnable stove_1Knob_1)
(IsTurnable stove_1Knob_2)
(IsTurnable stove_1Knob_3)
(IsTurnable stove_1Knob_4)
(IsTurnable sink_1Knob_1)
(IsPlacable Desk)
(IsPlacable Table)
(IsPlacable Tap)
(IsPlacable Stove)
(IsPlacable counter_1)
(IsPlacable stove_1Burner_1)
(IsPlacable stove_1Burner_2)
(IsPlacable stove_1Burner_3)
(IsPlacable stove_1Burner_4)
(IsPlacable sink_1)
(IsPlacable crockPot_1)
(IsPlacable microwave_1)
(IsOpenable Door)
(IsOpenable microwave_1)
(HasMilk Milk-Box)
(HasSugar Sugar-Box)
(HasCoffee Coffee-Powder)
(IsPlacable tap Below)
 (IsPlacable microwave Inside)
 (IsPlacable microwave_1 Inside)
 (IsPlacable table On)
 (IsPlacable desk On)
 (IsPlacable counter_1 On) (IsPlacable sink_1 On)(HasWater  crockPot_1)(Has ramen_1 crockPot_1)(IsNear microwave_1)(IsNear crockPot_1))(:goal (and (IsGrasping crockPot_1) )))
