namespace SecondScratch.ThreadSafe.Operations.Common
{
    public enum OperationTypes : long
    {
        UserDefinedTypes = 0b11111111_11111111_11111111_11111111_00000000_00000000_00000000_0000000,
        
        Action = 0b1,
        SubType = 0b1_00000000,
        Type = 0b1_00000000_00000000,
        
        #region Interaction
        
        Interaction = Type * 1,
        

        #region ContainerInteraction
        
        ContainerInteraction = Interaction + SubType * 1,
        
        ContainerInteractionAdd = ContainerInteraction + Action * 1,
        ContainerInteractionRemove = ContainerInteraction + Action * 2,
        
        #endregion
        
        #endregion
    }
}