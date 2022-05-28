// Pakon.IntBits
using Pakon;

public class IntBits
{
    private int m_nBits = 0;

    public int Bits
    {
        set
        {
            m_nBits = value;
        }
    }

    public bool this[int nIndex]
    {
        get
        {
            return (m_nBits & (1 << nIndex)) != 0;
        }
        set
        {
            if (value)
            {
                m_nBits |= 1 << nIndex;
            }
            else
            {
                m_nBits &= ~(1 << nIndex);
            }
        }
    }

    public void Empty()
    {
        m_nBits = 0;
    }

    public bool IsEmpty()
    {
        return m_nBits == 0;
    }

    public void Equals(IntBits ib)
    {
        m_nBits = ib.m_nBits;
    }

    public IntBits Clone()
    {
        IntBits intBits = new IntBits();
        intBits.m_nBits = m_nBits;
        return intBits;
    }
}
