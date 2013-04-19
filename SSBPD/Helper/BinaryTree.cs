using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SSBPD.Helper
{
    public class BinaryTree<T>
    {
        public T node;
        public BinaryTree<T> leftChild = null;
        public BinaryTree<T> rightChild = null;

        public BinaryTree(T element)
        {
            node = element;
        }

        public void AddLeftChild(T leftChild)
        {
            this.leftChild = new BinaryTree<T>(leftChild);
        }
        public void AddRightChild(T rightChild)
        {
            this.rightChild = new BinaryTree<T>(rightChild);
        }
        public void AddLeftChild(BinaryTree<T> leftChild)
        {
            this.leftChild = leftChild;
        }
        public void AddRightChild(BinaryTree<T> rightChild)
        {
            this.rightChild = rightChild;
        }
        public void SetLeftValue(T leftValue)
        {
            this.leftChild.node = leftValue;
        }
        public void SetRightValue(T rightValue)
        {
            this.rightChild.node = rightValue;
        }
    }
}